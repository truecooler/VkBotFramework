using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
		    var peerId = eventArgs.message.PeerId;
			var fromId = eventArgs.message.FromId;
			var text = eventArgs.message.Text;

			instanse.Logger.LogInformation($"new message captured. peerId: {peerId},userId: {fromId}, text: {text}");
		    instanse.Api.Messages.Send(new MessagesSendParams()
		    {
			    RandomId = Environment.TickCount,
				PeerId = eventArgs.message.PeerId,
				Message = $"{fromId.Value}, i have captured your message: '{text}'. its length is {text.Length}. number of spaces: {text.Count(x => x == ' ')}" 
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
