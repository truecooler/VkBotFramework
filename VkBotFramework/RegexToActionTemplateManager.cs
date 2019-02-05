using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VkBotFramework.Abstractions;
using VkBotFramework.Models;
using VkNet.Model;
using VkNet.Model.Keyboard;

namespace VkBotFramework
{
	public class RegexToActionTemplateManager : IRegexToActionTemplateManager
	{
		private object _regexToActionTemplatesLock = new object();
		private List<RegexToActionTemplate> _regexToActionTemplates = new List<RegexToActionTemplate>();

		//public void Register(string incomingMessageRegexPattern, string responseMessage,
		//	MessageKeyboard messageKeyboard = null,
		//	RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
		//{
		//	_regexToActionTemplates.Add(new RegexToActionTemplate(incomingMessageRegexPattern, responseMessage,
		//		messageKeyboard, incomingMessageRegexPatternOptions));
		//}

		//public void Register(string incomingMessageRegexPattern, List<string> responseMessages,
		//	MessageKeyboard messageKeyboard = null,
		//	RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
		//{
		//	_regexToActionTemplates.Add(new RegexToActionTemplate(incomingMessageRegexPattern, responseMessages,
		//		messageKeyboard, incomingMessageRegexPatternOptions));
		//}

		//public void Register(string incomingMessageRegexPattern, Action<VkBot, Message> callback,
		//	RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
		//{
		//	_regexToActionTemplates.Add(new RegexToActionTemplate(incomingMessageRegexPattern, callback,
		//		incomingMessageRegexPatternOptions));

		//}
		public void Unregister(RegexToActionTemplate template)
		{
			lock (_regexToActionTemplatesLock)
			{
				_regexToActionTemplates.Remove(template);
			}
		}
		public void Unregister(string incomingMessageRegexPattern, long peerId = 0)
		{
			lock (_regexToActionTemplatesLock)
			{
				_regexToActionTemplates.RemoveAll(x => x.IncomingMessageRegexPattern == incomingMessageRegexPattern
				                                       && x.PeerId == peerId);
			}
		}

		public void Register(RegexToActionTemplate template)
		{
			if (template == null)
			{
				throw new ArgumentNullException(nameof(template));
			}

			lock (_regexToActionTemplatesLock)
			{
				_regexToActionTemplates.Add(template);
			}

		}

		public IEnumerable<RegexToActionTemplate> SearchTemplatesMatchingMessage(Message message)
		{
			lock (_regexToActionTemplatesLock)
			{
				return _regexToActionTemplates.Where(x =>
					(x.PeerId == message.PeerId || x.PeerId == 0)
					&&
					Regex.IsMatch(message.Text, x.IncomingMessageRegexPattern, x.IncomingMessageRegexPatternOptions)
				);
			}
		}
	}
}
