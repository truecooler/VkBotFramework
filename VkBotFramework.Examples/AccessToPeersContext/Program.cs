using Microsoft.Extensions.Logging;
using System;
using VkBotFramework;
using VkBotFramework.Examples;
using VkBotFramework.Models;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace AccessToPeerContext
{
	class Program
	{
		static void Main(string[] args)
		{
			ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
			ILogger<VkBot> logger = loggerFactory.CreateLogger<VkBot>();

			ExampleSettings settings = ExampleSettings.TryToLoad(logger);

			VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl, logger);

			var keyboard = new KeyboardBuilder().SetOneTime().AddButton("тык", "").AddButton("тыдыщ","").Build();

			bot.TemplateManager.Register(new RegexToActionTemplate("тык", (sender, eventArgs) =>
				{
					PeerContext context = eventArgs.PeerContext;
					long peerId = eventArgs.Message.PeerId.Value;

					if (!context.Vars.ContainsKey("тыки"))
						context.Vars["тыки"] = 0;

					sender.Api.Messages.Send(new MessagesSendParams()
					{
						Keyboard = keyboard,
						PeerId = peerId,
						Message = $"тык номер {context.Vars["тыки"]++}",
						RandomId = Math.Abs(Environment.TickCount)
					});

				}
			));


			bot.TemplateManager.Register(new RegexToActionTemplate("тыдыщ", (sender, eventArgs) =>
			{
				if (!eventArgs.PeerContext.GlobalVars.ContainsKey("тыдыщи"))
					eventArgs.PeerContext.GlobalVars["тыдыщи"] = 0;

				sender.Api.Messages.Send(new MessagesSendParams()
				{
					Keyboard = keyboard,
					PeerId = eventArgs.Message.PeerId,
					Message = $"глобальный, междиалоговый тыдыщ номер {eventArgs.PeerContext.GlobalVars["тыдыщи"]++}",
					RandomId = Math.Abs(Environment.TickCount)
				});
			}));

			bot.Start();
			bot.Dispose();

		}
	}
}
