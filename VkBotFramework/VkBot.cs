using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums;
using VkNet.Model.RequestParams;
using VkNet.Model;
using VkNet.Exception;
using VkNet.Model.GroupUpdate;
using VkNet.Enums.SafetyEnums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace VkBotFramework
{
	public partial class VkBot : IDisposable
	{

		public event EventHandler<GroupUpdateReceivedEventArgs> OnGroupUpdateReceived;
		public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

		public VkApi Api = null;
		public LongPollServerResponse PollSettings = null;

		public long GroupId = 0;
		public string GroupUrl = string.Empty;

		private ILogger<VkBot> Logger;

		List<PhraseTemplate> PhraseTemplates = new List<PhraseTemplate>();

		public VkBot(IServiceCollection serviceCollection = null)
		{
			this.SetupDependencies(serviceCollection);
		}
		public VkBot(string accessToken, string groupUrl, IServiceCollection serviceCollection = null) : this(serviceCollection)
		{
			this.Setup(accessToken, groupUrl);
		}

		public VkBot(ILogger<VkBot> logger)
		{
			var container = new ServiceCollection();

			if (logger != null)
			{
				container.TryAddSingleton(logger);
			}
			this.SetupDependencies(container);


		}

		public VkBot(string accessToken, string groupUrl, ILogger<VkBot> logger) : this(logger)
		{
			this.Setup(accessToken, groupUrl);
		}

		private void RegisterDefaultDependencies(IServiceCollection container)
		{
			if (container.All(x => x.ServiceType != typeof(ILogger<>)))
			{
				container.TryAddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
			}
		}

		public void Setup(string accessToken, string groupUrl)
		{
			
			Api.RequestsPerSecond = 20;//лимит для группового access token
			Api.Authorize(new ApiAuthParams
			{
				AccessToken = accessToken
			});

			this.GroupUrl = groupUrl;
			this.GroupId = this.ResolveGroupId(groupUrl);

			Api.RestClient.Timeout = TimeSpan.FromSeconds(30);
			//ServicePointManager.UseNagleAlgorithm = false;
			//ServicePointManager.Expect100Continue = false;
			ServicePointManager.DefaultConnectionLimit = 20;
			//ServicePointManager.EnableDnsRoundRobin = true;
			//ServicePointManager.ReusePort = true;
		}

		private void SetupDependencies(IServiceCollection serviceCollection = null)
		{
			var container = serviceCollection ?? new ServiceCollection();
			this.RegisterDefaultDependencies(container);
			IServiceProvider serviceProvider = container.BuildServiceProvider();
			this.Logger = serviceProvider.GetService<ILogger<VkBot>>();
			Api = new VkApi(container);
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
			PollSettings = Api.Groups.GetLongPollServer((ulong)this.GroupId);
			this.Logger.LogInformation($"VkBot: LongPoolSettings received. ts: {PollSettings.Ts}");
		}

		public void RegisterPhraseTemplate(string regexPattern, string answer, RegexOptions phraseRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			PhraseTemplates.Add(new PhraseTemplate(regexPattern, answer, phraseRegexPatternOptions));
		}
		public void RegisterPhraseTemplate(string regexPattern, List<string> answers, RegexOptions phraseRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			PhraseTemplates.Add(new PhraseTemplate(regexPattern, answers, phraseRegexPatternOptions));
		}

		public void RegisterPhraseTemplate(string regexPattern, Action<Message> callback, RegexOptions phraseRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			PhraseTemplates.Add(new PhraseTemplate(regexPattern, callback, phraseRegexPatternOptions));
		}

		private void SearchPhraseAndHandle(Message message)
		{
			foreach (PhraseTemplate pair in PhraseTemplates)
			{
				Regex regex = new Regex(pair.PhraseRegexPattern, pair.PhraseRegexPatternOptions);
				if (regex.IsMatch(message.Text))
				{

					if (pair.Callback == null)
						//TODO: сделать этот вызов асинхронным
						Api.Messages.Send(new MessagesSendParams { Message = pair.Answers[new Random().Next(0, pair.Answers.Count)], PeerId = message.PeerId });
					else
						pair.Callback(message);
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
					SearchPhraseAndHandle(update.Message);
				}

			}
		}

		T CheckLongPollResponseForErrorsAndHandle<T>(Task<T> task)
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
				this.Logger.LogWarning("CheckLongPollResponseForErrorsAndHandle() : task.IsCanceled, possibly timeout reached");
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
					throw ex;
				}
			}
		}



		public async Task StartAsync()
		{
			this.SetupLongPoll();
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
							Wait = 25
						}).ContinueWith(CheckLongPollResponseForErrorsAndHandle).ConfigureAwait(false);
					if (longPollResponse == default(BotsLongPollHistoryResponse))
						continue;
					//Console.WriteLine(JsonConvert.SerializeObject(longPollResponse));
					this.ProcessLongPollEvents(longPollResponse);
					PollSettings.Ts = longPollResponse.Ts;
				}
				catch (Exception ex)
				{
					this.Logger.LogError(ex.Message);
					throw;
				}
			}
		}

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
