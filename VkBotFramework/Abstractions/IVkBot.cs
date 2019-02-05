using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VkBotFramework.Models;
using VkNet.Abstractions;

namespace VkBotFramework.Abstractions
{
	interface IVkBot : IDisposable
	{
		event EventHandler<GroupUpdateReceivedEventArgs> OnGroupUpdateReceived;
		event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
		event EventHandler OnBotStarted;
		IVkApi Api { get; }
		ILogger<VkBot> Logger { get; }
		IRegexToActionTemplateManager TemplateManager { get; }
		IPeerContextManager PeerContextManager { get;  }
		long GroupId { get; }
		string GroupUrl { get; }

		string FilteredGroupUrl { get; }

		Task StartAsync();
		void Start();
	}
}
