using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VkNet.Model;

namespace VkBotFramework.Models
{
	public class RegexToActionTemplate
	{
		public Action<VkBot, MessageReceivedEventArgs> Callback;

		public string IncomingMessageRegexPattern;
		public RegexOptions IncomingMessageRegexPatternOptions;
		public MessageKeyboard MessageKeyboard;
		public long PeerId;
		public List<string> ResponseMessages;

		public RegexToActionTemplate(string incomingMessageRegexPattern, string responseMessage,
			MessageKeyboard messageKeyboard = null,
			long peerId = 0,
			RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			if (incomingMessageRegexPattern == null)
				throw new ArgumentNullException(nameof(incomingMessageRegexPattern));
			if (string.IsNullOrEmpty(responseMessage))
				throw new ArgumentNullException(nameof(responseMessage));

			this.PeerId = peerId;
			this.IncomingMessageRegexPattern = incomingMessageRegexPattern;
			this.ResponseMessages = new List<string>();
			this.ResponseMessages.Add(responseMessage);
			this.IncomingMessageRegexPatternOptions = incomingMessageRegexPatternOptions;
			this.MessageKeyboard = messageKeyboard;
		}


		public RegexToActionTemplate(string incomingMessageRegexPattern, List<string> responseMessages,
			long peerId = 0,
			MessageKeyboard messageKeyboard = null,
			RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			if (incomingMessageRegexPattern == null)
				throw new ArgumentNullException(nameof(incomingMessageRegexPattern));
			if (responseMessages == null)
				throw new ArgumentNullException(nameof(responseMessages));
			if (responseMessages.Count < 1)
				throw new ArgumentException("items count cant be less than 1", nameof(responseMessages));

			this.PeerId = peerId;
			this.IncomingMessageRegexPattern = incomingMessageRegexPattern;
			this.ResponseMessages = responseMessages;
			this.IncomingMessageRegexPatternOptions = incomingMessageRegexPatternOptions;
			this.MessageKeyboard = messageKeyboard;
		}

		public RegexToActionTemplate(string incomingMessageRegexPattern,
			Action<VkBot, MessageReceivedEventArgs> callback,
			long peerId = 0,
			RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
		{
			if (incomingMessageRegexPattern == null)
				throw new ArgumentNullException(nameof(incomingMessageRegexPattern));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			this.PeerId = peerId;
			this.IncomingMessageRegexPattern = incomingMessageRegexPattern;
			this.IncomingMessageRegexPatternOptions = incomingMessageRegexPatternOptions;
			this.Callback = callback;
		}

		public string GetRandomResponseMessage()
		{
			var rand = new Random();
			return this.ResponseMessages[rand.Next(0, this.ResponseMessages.Count)];
		}
	}
}