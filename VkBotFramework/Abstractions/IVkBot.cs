using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VkBotFramework.Models;
using VkNet.Abstractions;

namespace VkBotFramework.Abstractions
{
	interface IVkBot : IDisposable
	{
		IVkApi Api { get; }
		ILogger<VkBot> Logger { get; }
		IRegexToActionTemplateManager TemplateManager { get; }
		IPeerContextManager PeerContextManager { get; }
		long GroupId { get; }
		string GroupUrl { get; }

		string FilteredGroupUrl { get; }
		event EventHandler<GroupUpdateReceivedEventArgs> OnGroupUpdateReceived;
		event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
		event EventHandler OnBotStarted;

		Task StartAsync();
		void Start();
	}
}