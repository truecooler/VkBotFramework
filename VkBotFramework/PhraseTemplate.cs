using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VkNet.Model;

namespace VkBotFramework
{
	public partial class VkBot
	{
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
	}
}
