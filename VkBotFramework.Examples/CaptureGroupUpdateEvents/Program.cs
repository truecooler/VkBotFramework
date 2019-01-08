using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Configuration;
using VkBotFramework.Examples;
using VkNet.Enums.SafetyEnums;
using VkBotFramework.Models;
using VkNet.Model.RequestParams;
using VkBotFramework;

namespace CaptureGroupUpdateEvents
{
    class Program
    {
	    static void NewCommentHandler(object sender, GroupUpdateReceivedEventArgs eventArgs)
	    {
		    if (eventArgs.update.Type == GroupUpdateType.WallReplyNew)
		    {
				VkBot instanse = sender as VkBot;
			    var postId = eventArgs.update.WallReply.PostId;
			    var postTest = eventArgs.update.WallReply.Text;
			    instanse.Logger.LogInformation($"new comment under post. id:{postId}, text:{postTest}");
		    }
	    }
		static void Main(string[] args)
        {
			ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
			ILogger<VkBot> logger = loggerFactory.CreateLogger<VkBot>();

			ExampleSettings settings = ExampleSettings.TryToLoad(logger);

			VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl, logger);

			/* subscribe for anon lambda in order to receive any update */
			bot.OnGroupUpdateReceived += (sender, eventArgs) =>
			{
				logger.LogInformation(eventArgs.update.Type.ToString() + " event type triggered");
			};


			/* subscribe for static method in order to receive new comment update type */
			bot.OnGroupUpdateReceived += NewCommentHandler;


			bot.OnBotStarted += (sender, eventArgs) =>
			{
				bot.Api.Wall.CreateComment(new WallCreateCommentParams() { OwnerId = -bot.GroupId, PostId = 1, Message = "test" });
			};

			bot.Start();
			//bot.Dispose();
			Console.ReadLine();
		}
    }
}
