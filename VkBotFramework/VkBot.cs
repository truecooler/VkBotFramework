using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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



namespace VkBotFramework
{
    public class VkBot
    {
		public class GroupUpdateReceivedEventArgs : EventArgs
		{
			public GroupUpdateReceivedEventArgs(GroupUpdate update)
			{
				this.update = update;
			}
			public GroupUpdate update;
		}


		public class MessageReceivedEventArgs : EventArgs
		{
			public MessageReceivedEventArgs(Message message)
			{
				this.message = message;
			}
			public Message message;
		}

		public event EventHandler<GroupUpdateReceivedEventArgs> OnGroupUpdateReceived;
		public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

		public VkApi Api = null;
		public LongPollServerResponse PollSettings = null;

		public VkBot(string accessToken, string groupUrl)
		{

			this.GroupUrl = groupUrl;
			Api = new VkApi();
			Api.RequestsPerSecond = 20;//лимит для группового access token
			PhraseTemplates = new List<PhraseTemplate>();
			Api.Authorize(new ApiAuthParams
			{
				AccessToken = accessToken
			});
			Api.RestClient.Timeout = TimeSpan.FromSeconds(30);
		}

		public ulong GroupId = 0;
		public string GroupUrl = string.Empty;

		private void ResolveGroupId()
		{
			this.GroupUrl = Regex.Replace(this.GroupUrl, ".*/", "");
			VkObject result = this.Api.Utils.ResolveScreenName(this.GroupUrl);
			if (result.Type != VkObjectType.Group) throw new VkApiException("GroupUrl не указывает на группу.");
			this.GroupId = (ulong)result.Id;
			Console.WriteLine($"GroupId resolved. id: {this.GroupId}");
		}
		
		private void SetupLongPoll()
		{
			if (this.GroupId == 0) this.ResolveGroupId();
			PollSettings = Api.Groups.GetLongPollServer(this.GroupId);
			Console.WriteLine($"LongPoolSettings updated. ts: {PollSettings.Ts}");
		}

		class PhraseTemplate
		{
			public PhraseTemplate(string phraseRegexPattern, string answer, RegexOptions phraseRegexPatternOptions)
			{
				this.PhraseRegexPattern = phraseRegexPattern;
				this.Answers = new List<string>();
				this.Answers.Add(answer);
				this.PhraseRegexPatternOptions = phraseRegexPatternOptions;
			}


			public PhraseTemplate(string phraseRegexPattern, List<string> answers, RegexOptions phraseRegexPatternOptions)
			{
				this.PhraseRegexPattern = phraseRegexPattern;
				this.Answers = answers;
				this.PhraseRegexPatternOptions = phraseRegexPatternOptions;
			}

			public PhraseTemplate(string phraseRegexPattern, Action<Message> callback, RegexOptions phraseRegexPatternOptions)
			{
				this.PhraseRegexPattern = phraseRegexPattern;
				this.PhraseRegexPatternOptions = phraseRegexPatternOptions;
				this.Callback = callback;
			}

			public string PhraseRegexPattern;
			public List<string> Answers = null;
			public RegexOptions PhraseRegexPatternOptions;
			public Action<Message> Callback = null;
		}

		List<PhraseTemplate> PhraseTemplates;

		public void RegisterPhraseTemplate(string regexPattern, string answer, RegexOptions phraseRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			PhraseTemplates.Add(new PhraseTemplate( regexPattern, answer, phraseRegexPatternOptions));
		}
		public void RegisterPhraseTemplate(string regexPattern, List<string> answers, RegexOptions phraseRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			PhraseTemplates.Add(new PhraseTemplate(regexPattern, answers, phraseRegexPatternOptions));
		}

		public void RegisterPhraseTemplate(string regexPattern, Action<Message> callback, RegexOptions phraseRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			PhraseTemplates.Add(new PhraseTemplate(regexPattern, callback, phraseRegexPatternOptions));
		}

		private async void SearchPhraseAndHandle(Message message)
		{
			foreach (PhraseTemplate pair in PhraseTemplates)
			{
				Regex regex = new Regex(pair.PhraseRegexPattern, pair.PhraseRegexPatternOptions);
				if (regex.IsMatch(message.Text))
				{

					if (pair.Callback == null)
						await Api.Messages.SendAsync(new MessagesSendParams { Message = pair.Answers[new Random().Next(0, pair.Answers.Count)], PeerId = message.PeerId });
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

				Console.WriteLine(task.Exception.Message);
				throw task.Exception;

			}
			else if (task.IsCanceled)
			{
				Console.WriteLine("CheckLongPollResponseForErrorsAndHandle() : task.IsCanceled, possibly timeout reached");
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
					Console.WriteLine(ex.Message);
					throw ex;
				}
			}
		}

		

		public void Start()
		{
			this.SetupLongPoll();
			while (true)
			{
				try
				{
					BotsLongPollHistoryResponse longPollResponse = Api.Groups.GetBotsLongPollHistoryAsync(
						new BotsLongPollHistoryParams
						{
							Key = PollSettings.Key,
							Server = PollSettings.Server,
							Ts = PollSettings.Ts,
							Wait = 25
						}).ContinueWith(CheckLongPollResponseForErrorsAndHandle).GetAwaiter().GetResult();
					if (longPollResponse == default(BotsLongPollHistoryResponse))
						continue;
					//Console.WriteLine(JsonConvert.SerializeObject(longPollResponse));
					this.ProcessLongPollEvents(longPollResponse);
					PollSettings.Ts = longPollResponse.Ts;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}



	}
}
