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
	    public ConcurrentBag<RegexToActionTemplate> RegexToActionTemplates = new ConcurrentBag<RegexToActionTemplate>();

	    public void Register(string incomingMessageRegexPattern, string responseMessage,
		    MessageKeyboard messageKeyboard = null,
		    RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
	    {
		    RegexToActionTemplates.Add(new RegexToActionTemplate(incomingMessageRegexPattern, responseMessage,
			    messageKeyboard, incomingMessageRegexPatternOptions));
	    }

	    public void Register(string incomingMessageRegexPattern, List<string> responseMessages,
		    MessageKeyboard messageKeyboard = null,
		    RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
	    {
		    RegexToActionTemplates.Add(new RegexToActionTemplate(incomingMessageRegexPattern, responseMessages,
			    messageKeyboard, incomingMessageRegexPatternOptions));
	    }

	    public void Register(string incomingMessageRegexPattern, Action<VkBot, Message> callback,
		    RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase)
	    {
		    RegexToActionTemplates.Add(new RegexToActionTemplate(incomingMessageRegexPattern, callback,
			    incomingMessageRegexPatternOptions));

	    }

	    public void Register(RegexToActionTemplate template)
	    {
		    if (template == null)
		    {
			    throw new ArgumentNullException(nameof(template));
			}
		    RegexToActionTemplates.Add(template);

	    }

	    public IEnumerable<RegexToActionTemplate> SearchTemplatesMatchingMessage(Message message)
	    {
		    return RegexToActionTemplates.Where(x =>
			    Regex.IsMatch(message.Text, x.IncomingMessageRegexPattern, x.IncomingMessageRegexPatternOptions));
	    }
    }
}
