using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VkNet.Model;
using VkNet.Model.Keyboard;

namespace VkBotFramework.Models
{ 

		public class RegexToActionTemplate
		{
			public RegexToActionTemplate(string incomingMessageRegexPattern, string responseMessage,
				MessageKeyboard messageKeyboard = null,
				RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
			{
				if (incomingMessageRegexPattern == null)
					throw new ArgumentNullException(nameof(incomingMessageRegexPattern));
				if (string.IsNullOrEmpty(responseMessage))
					throw new ArgumentNullException(nameof(responseMessage));


				this.IncomingMessageRegexPattern = incomingMessageRegexPattern;
				this.ResponseMessages = new List<string>();
				this.ResponseMessages.Add(responseMessage);
				this.IncomingMessageRegexPatternOptions = incomingMessageRegexPatternOptions;
				this.MessageKeyboard = messageKeyboard;
			}


			public RegexToActionTemplate(string incomingMessageRegexPattern, List<string> responseMessages,
				MessageKeyboard messageKeyboard = null,
				RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
			{
				if (incomingMessageRegexPattern == null)
					throw new ArgumentNullException(nameof(incomingMessageRegexPattern));
				if (responseMessages == null)
					throw new ArgumentNullException(nameof(responseMessages));
				if (responseMessages.Count < 1)
					throw new ArgumentException("items count cant be less than 1", nameof(responseMessages));

				this.IncomingMessageRegexPattern = incomingMessageRegexPattern;
				this.ResponseMessages = responseMessages;
				this.IncomingMessageRegexPatternOptions = incomingMessageRegexPatternOptions;
				this.MessageKeyboard = messageKeyboard;
			}

			public RegexToActionTemplate(string incomingMessageRegexPattern, Action<VkBot, Message> callback,
				RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
			{
				if (incomingMessageRegexPattern == null)
					throw new ArgumentNullException(nameof(incomingMessageRegexPattern));
				if (callback == null)
					throw new ArgumentNullException(nameof(callback));

				this.IncomingMessageRegexPattern = incomingMessageRegexPattern;
				this.IncomingMessageRegexPatternOptions = incomingMessageRegexPatternOptions;
				this.Callback = callback;
			}

			public string GetRandomResponseMessage()
			{
				var rand = new Random();
				return this.ResponseMessages[rand.Next(0, this.ResponseMessages.Count)];
			}

			public string IncomingMessageRegexPattern;
			public List<string> ResponseMessages = null;
			public RegexOptions IncomingMessageRegexPatternOptions;
			public Action<VkBot,Message> Callback = null;
			public MessageKeyboard MessageKeyboard = null;
		}
}