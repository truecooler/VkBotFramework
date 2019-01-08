using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Collections.Concurrent;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums;
using VkNet.Model.RequestParams;
using VkNet.Model;
using VkNet.Exception;
using VkNet.Model.GroupUpdate;
using VkNet.Enums.SafetyEnums;
using VkNet.Abstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VkBotFramework.Abstractions;
using VkBotFramework.Models;
using VkNet.Model.Keyboard;

namespace VkBotFramework
{
	public partial class VkBot : IVkBot
	{

		public event EventHandler<GroupUpdateReceivedEventArgs> OnGroupUpdateReceived;
		public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
		public event EventHandler OnBotStarted;

		public IVkApi Api { get; private set; }
		public ILogger<VkBot> Logger { get; private set; }
		public IRegexToActionTemplateManager TemplateManager { get; private set; }

		private LongPollServerResponse PollSettings { get; set; }

		public long GroupId { get; private set; }
		public string GroupUrl { get; private set; }

		private int LongPollTimeoutWaitSeconds { get; set; } = 25;



		//public VkBot(IServiceCollection serviceCollection = null)
		//{
		//	this.SetupDependencies(serviceCollection);
		//}
		public VkBot(string accessToken, string groupUrl, IServiceCollection serviceCollection = null,
			int longPollTimeoutWaitSeconds = 25) // : this(serviceCollection)
		{
			this.SetupDependencies(serviceCollection);
			this.Setup(accessToken, groupUrl, longPollTimeoutWaitSeconds);

		}

		//public VkBot(ILogger<VkBot> logger)
		//{
		//	var container = new ServiceCollection();

		//	if (logger != null)
		//	{
		//		container.TryAddSingleton(logger);
		//	}
		//	this.SetupDependencies(container);
		//}

		public VkBot(string accessToken, string groupUrl, ILogger<VkBot> logger,
			int longPollTimeoutWaitSeconds = 25) // : this(logger)
		{
			var container = new ServiceCollection();
			if (logger != null)
			{
				container.TryAddSingleton(logger);
			}

			this.SetupDependencies(container);

			this.Setup(accessToken, groupUrl, longPollTimeoutWaitSeconds);

		}

		private void RegisterDefaultDependencies(IServiceCollection container)
		{
			if (container.All(x => x.ServiceType != typeof(ILogger<>)))
			{
				container.TryAddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
			}

			if (container.All(x => x.ServiceType != typeof(IRegexToActionTemplateManager)))
			{
				container.TryAddSingleton(typeof(IRegexToActionTemplateManager), typeof(RegexToActionTemplateManager));
			}

			if (container.All(x => x.ServiceType != typeof(IVkApi)))
			{
				var vkApiByDefault = new VkApi();
				vkApiByDefault.RestClient.Timeout = TimeSpan.FromSeconds(30);
				vkApiByDefault.RequestsPerSecond = 20; //лимит для группового access token
				container.TryAddSingleton<IVkApi>(x => vkApiByDefault);
			}
		}

		private void Setup(string accessToken, string groupUrl, int longPollTimeoutWaitSeconds = 25)
		{
			if (string.IsNullOrEmpty(accessToken))
				throw new ArgumentNullException(nameof(accessToken));
			if (string.IsNullOrEmpty(groupUrl))
				throw new ArgumentNullException(nameof(groupUrl));

			Api.Authorize(new ApiAuthParams
			{
				AccessToken = accessToken
			});

			this.LongPollTimeoutWaitSeconds = longPollTimeoutWaitSeconds;
			this.GroupUrl = groupUrl;
			this.GroupId = this.ResolveGroupId(groupUrl);

			//Api.RestClient.Timeout = TimeSpan.FromSeconds(30);

			//ServicePointManager.UseNagleAlgorithm = false;
			//ServicePointManager.Expect100Continue = false;
			ServicePointManager.DefaultConnectionLimit = 20; //ограничение параллельных соединений для HttpClient
			//ServicePointManager.EnableDnsRoundRobin = true;
			//ServicePointManager.ReusePort = true;
		}

		private void SetupDependencies(IServiceCollection serviceCollection = null)
		{
			var container = serviceCollection ?? new ServiceCollection();
			this.RegisterDefaultDependencies(container);
			IServiceProvider serviceProvider = container.BuildServiceProvider();
			this.Logger = serviceProvider.GetService<ILogger<VkBot>>();
			this.Api = serviceProvider.GetService<IVkApi>(); //new VkApi(container);
			this.TemplateManager = serviceProvider.GetService<IRegexToActionTemplateManager>();
			this.Logger.LogInformation("Все зависимости подключены.");
		}


		private long ResolveGroupId(string groupUrl)
		{
			VkObject result = this.Api.Utils.ResolveScreenName(Regex.Replace(groupUrl, ".*/", ""));
			if (result.Type != VkObjectType.Group) throw new VkApiException("GroupUrl не указывает на группу.");
			long groupId = result.Id.Value;
			this.Logger.LogInformation($"VkBot: GroupId resolved. id: {groupId}");
			return groupId;
		}

		private void SetupLongPoll()
		{
			PollSettings = Api.Groups.GetLongPollServer((ulong) this.GroupId);
			this.Logger.LogInformation($"VkBot: LongPoolSettings received. ts: {PollSettings.Ts}");
		}



		private void SearchTemplatesMatchingMessageAndHandle(Message message)
		{
			var rand = new Random();
			foreach (RegexToActionTemplate matchingTemplate in
				this.TemplateManager.SearchTemplatesMatchingMessage(message))
			{
				if (matchingTemplate.Callback == null)
				{
					//TODO: make this call async
					Api.Messages.Send(new MessagesSendParams
					{
						Message = matchingTemplate.GetRandomResponseMessage(),
						PeerId = message.PeerId,
						RandomId = rand.Next(1, Int32.MaxValue),
						Keyboard = matchingTemplate.MessageKeyboard
					});
				}
				else
				{
					matchingTemplate.Callback(this, message);
				}
			}
		}

		private void ProcessLongPollEvents(BotsLongPollHistoryResponse pollResponse)
		{

			foreach (GroupUpdate update in pollResponse.Updates)
			{
				OnGroupUpdateReceived?.Invoke(this, new GroupUpdateReceivedEventArgs(update));
				if (update.Type == GroupUpdateType.MessageNew)
				{
					OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(update.Message));
					this.SearchTemplatesMatchingMessageAndHandle(update.Message);
				}

			}
		}

		private T CheckLongPollResponseForErrorsAndHandle<T>(Task<T> task)
		{
			if (task.IsFaulted)
			{
				if (task.Exception is AggregateException ae)
				{
					foreach (Exception ex in ae.InnerExceptions)
					{
						if (ex is LongPollOutdateException lpoex)
						{
							PollSettings.Ts = lpoex.Ts;
							return default(T);
						}
						else if (ex is LongPollKeyExpiredException)
						{
							this.SetupLongPoll();
							return default(T);
						}
						else if (ex is LongPollInfoLostException)
						{
							this.SetupLongPoll();
							return default(T);
						}
						else
						{
							Console.WriteLine(ex.Message);
							throw ex;
						}
					}
				}

				this.Logger.LogError(task.Exception.Message);
				throw task.Exception;

			}
			else if (task.IsCanceled)
			{
				this.Logger.LogWarning(
					"CheckLongPollResponseForErrorsAndHandle() : task.IsCanceled, possibly timeout reached");
				return default(T);
			}
			else
			{
				try
				{
					return task.Result;
				}
				catch (Exception ex)
				{
					this.Logger.LogError(ex.Message);
					throw;
				}
			}
		}



		public async Task StartAsync()
		{
			this.SetupLongPoll();
			this.OnBotStarted?.Invoke(this, null);
			while (true)
			{
				try
				{
					BotsLongPollHistoryResponse longPollResponse = await Api.Groups.GetBotsLongPollHistoryAsync(
						new BotsLongPollHistoryParams
						{
							Key = PollSettings.Key,
							Server = PollSettings.Server,
							Ts = PollSettings.Ts,
							Wait = this.LongPollTimeoutWaitSeconds
						}).ContinueWith(CheckLongPollResponseForErrorsAndHandle).ConfigureAwait(false);
					if (longPollResponse == default(BotsLongPollHistoryResponse))
						continue;
					//Console.WriteLine(JsonConvert.SerializeObject(longPollResponse));
					this.ProcessLongPollEvents(longPollResponse);
					PollSettings.Ts = longPollResponse.Ts;
				}
				catch (Exception ex)
				{
					this.Logger.LogError(ex.Message + "\r\n" + ex.StackTrace);
					throw;
				}
			}
		}

		//TODO: ask your teamlead for better solution
		public void Start()
		{
			this.StartAsync().GetAwaiter().GetResult();
		}


		public void Dispose()
		{
			Api.Dispose();
		}

	}
}
