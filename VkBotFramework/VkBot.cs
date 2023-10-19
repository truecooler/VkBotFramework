using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VkBotFramework.Abstractions;
using VkBotFramework.Exceptions;
using VkBotFramework.Models;
using VkNet;
using VkNet.Abstractions;
using VkNet.Enums;
using VkNet.Enums.StringEnums;
using VkNet.Exception;
using VkNet.Model;

namespace VkBotFramework
{
	public partial class VkBot : IVkBot
	{
		public VkBot(string accessToken, string groupUrl, IServiceCollection serviceCollection = null,
			int longPollTimeoutWaitSeconds = 25)
		{
			this.SetupDependencies(serviceCollection);
			this.SetupVkBot(accessToken, groupUrl, longPollTimeoutWaitSeconds);
		}

		public VkBot(string accessToken, string groupUrl, ILogger<VkBot> logger,
			int longPollTimeoutWaitSeconds = 25)
		{
			var container = new ServiceCollection();
			if (logger != null)
			{
				container.TryAddSingleton(logger);
			}

			this.SetupDependencies(container);

			this.SetupVkBot(accessToken, groupUrl, longPollTimeoutWaitSeconds);
		}

		private LongPollServerResponse _pollSettings { get; set; }

		private int _longPollTimeoutWaitSeconds { get; set; } = 25;

		public event EventHandler<GroupUpdateReceivedEventArgs> OnGroupUpdateReceived;
		public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
		public event EventHandler OnBotStarted;

		public IVkApi Api { get; private set; }
		public ILogger<VkBot> Logger { get; private set; }
		public IRegexToActionTemplateManager TemplateManager { get; private set; }

		public IPeerContextManager PeerContextManager { get; private set; }

		public long GroupId { get; private set; }
		public string GroupUrl { get; private set; }

		public string FilteredGroupUrl { get; private set; }


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
							Key = _pollSettings.Key,
							Server = _pollSettings.Server,
							Ts = _pollSettings.Ts,
							Wait = this._longPollTimeoutWaitSeconds
						}).ContinueWith(CheckLongPollResponseForErrorsAndHandle).ConfigureAwait(false);
					if (longPollResponse == default(BotsLongPollHistoryResponse))
						continue;

					this.ProcessLongPollEvents(longPollResponse);
					_pollSettings.Ts = longPollResponse.Ts;
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

			if (container.All(x => x.ServiceType != typeof(IPeerContextManager)))
			{
				container.TryAddSingleton(typeof(IPeerContextManager), typeof(PeerContextManager));
			}

			if (container.All(x => x.ServiceType != typeof(IPeerContextManager)))
			{
				container.TryAddSingleton(typeof(IPeerContextManager), typeof(PeerContextManager));
			}

			if (container.All(x => x.ServiceType != typeof(HttpClient)))
			{
				container.AddSingleton(provider => new HttpClient() {Timeout = TimeSpan.FromSeconds(30)});
			}

			if (container.All(x => x.ServiceType != typeof(IVkApi)))
			{
				var vkApiByDefault = new VkApi();
				vkApiByDefault.RequestsPerSecond = 20; //лимит для группового access token
				container.TryAddSingleton<IVkApi>(x => vkApiByDefault);
			}
		}

		private void SetupVkBot(string accessToken, string groupUrl, int longPollTimeoutWaitSeconds = 25)
		{
			if (string.IsNullOrEmpty(accessToken))
				throw new ArgumentNullException(nameof(accessToken));
			if (string.IsNullOrEmpty(groupUrl))
				throw new ArgumentNullException(nameof(groupUrl));

			Api.Authorize(new ApiAuthParams
			{
				AccessToken = accessToken
			});

			this._longPollTimeoutWaitSeconds = longPollTimeoutWaitSeconds;
			this.GroupUrl = groupUrl;
			this.GroupId = this.ResolveGroupId(groupUrl);

			ServicePointManager.DefaultConnectionLimit = 20; //ограничение параллельных соединений для HttpClient

		}

		private void SetupDependencies(IServiceCollection serviceCollection = null)
		{
			var container = serviceCollection ?? new ServiceCollection();

			this.RegisterDefaultDependencies(container);

			IServiceProvider serviceProvider = container.BuildServiceProvider();

			this.Logger = serviceProvider.GetService<ILogger<VkBot>>();

			this.Api = serviceProvider.GetService<IVkApi>(); //new VkApi(container);

			this.TemplateManager = serviceProvider.GetService<IRegexToActionTemplateManager>();

			this.PeerContextManager = serviceProvider.GetService<IPeerContextManager>();

			this.Logger.LogInformation("Все зависимости подключены.");
		}


		private long ResolveGroupId(string groupUrl)
		{
			this.FilteredGroupUrl = Regex.Replace(groupUrl, ".*/", "");

			VkObject result = this.Api.Utils.ResolveScreenName(this.FilteredGroupUrl);

			if (result == null || !result.Id.HasValue)
				throw new GroupNotResolvedException($"группа '{groupUrl}' не существует.");

			if (result.Type != VkObjectType.Group)
				throw new GroupNotResolvedException("GroupUrl не указывает на группу.");

			long groupId = result.Id.Value;

			this.Logger.LogInformation($"VkBot: GroupId resolved. id: {groupId}");
			return groupId;
		}

		private void SetupLongPoll()
		{
			_pollSettings = Api.Groups.GetLongPollServer((ulong) this.GroupId);
			this.Logger.LogInformation($"VkBot: LongPoolSettings received. ts: {_pollSettings.Ts}");
		}


		private void SearchTemplatesMatchingMessageAndHandle(Message message, PeerContext peerContext)
		{
			var rand = new Random();
			foreach (RegexToActionTemplate matchingTemplate in
				//работа происходит с копией коллекции, поскольку оригинальная может быть изменена callback'ом
				this.TemplateManager.SearchTemplatesMatchingMessage(message).ToList())
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
					matchingTemplate.Callback(this, new MessageReceivedEventArgs(message, peerContext));
				}
			}
		}

		private void ProcessLongPollEvents(BotsLongPollHistoryResponse pollResponse)
		{
			foreach (GroupUpdate update in pollResponse.Updates)
			{
				OnGroupUpdateReceived?.Invoke(this, new GroupUpdateReceivedEventArgs(update,
					this.PeerContextManager.GlobalVars));

				if (update.Type.Value == GroupUpdateType.MessageNew)
				{
					var messageNewUpdate = update.Instance as MessageNew;
					long peerId = messageNewUpdate.Message.PeerId.Value;
					PeerContext peerContext = null;

					if (!this.PeerContextManager.Peers.TryGetValue(peerId, out peerContext))
					{
						peerContext = new PeerContext(this.PeerContextManager.GlobalVars);
						this.PeerContextManager.Peers.Add(peerId, peerContext);
					}

					OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageNewUpdate.Message, peerContext));
					this.SearchTemplatesMatchingMessageAndHandle(messageNewUpdate.Message, peerContext);
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
							_pollSettings.Ts = lpoex.Ts;
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
	}
}