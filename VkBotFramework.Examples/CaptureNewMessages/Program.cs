using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using VkBotFramework;
using VkBotFramework.Examples;
using VkBotFramework.Models;
using VkNet.Model.RequestParams;

namespace CaptureNewMessages
{
	class Program
	{
		static void NewMessageHandler(object sender, MessageReceivedEventArgs eventArgs)
		{
			VkBot instanse = sender as VkBot;
			var peerId = eventArgs.Message.PeerId;
			var fromId = eventArgs.Message.FromId;
			var text = eventArgs.Message.Text;

			instanse.Logger.LogInformation($"new message captured. peerId: {peerId},userId: {fromId}, text: {text}");
			instanse.Api.Messages.Send(new MessagesSendParams()
			{
				RandomId = Environment.TickCount,
				PeerId = eventArgs.Message.PeerId,
				Message =
					$"{fromId.Value}, i have captured your message: '{text}'. its length is {text.Length}. number of spaces: {text.Count(x => x == ' ')}"
			});
		}

		static void Main(string[] args)
		{
			ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
			ILogger<VkBot> logger = loggerFactory.CreateLogger<VkBot>();

			ExampleSettings settings = ExampleSettings.TryToLoad(logger);

			VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl, logger);

			bot.OnMessageReceived += NewMessageHandler;

			bot.Start();

			Console.ReadLine();
		}
	}
}