using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using VkBotFramework;
using VkBotFramework.Examples;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace MessagesWithKeyboard
{
	class Program
	{
		static void Main(string[] args)
		{
			ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
			ILogger<VkBot> logger = loggerFactory.CreateLogger<VkBot>();

			ExampleSettings settings = ExampleSettings.TryToLoad(logger);

			VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl, logger);

			var isBotStarted = false;

			int validAnswer = 0;


			bot.TemplateManager.Register("матеша", (sender, eventArgs) =>
			{
				isBotStarted = true;
				var rand = new Random();
				var firstNum = rand.Next(1, 100);
				var secondNum = rand.Next(1, 100);

				validAnswer = firstNum + secondNum;

				int buttonsCount = 4;

				int validButtonIndex = rand.Next(buttonsCount);
				var keyboard = new KeyboardBuilder();
				for (int i = 0; i < buttonsCount; i++)
				{
					if (i == validButtonIndex)
						keyboard.AddButton((validAnswer).ToString(), i.ToString());
					else
						keyboard.AddButton(rand.Next(200).ToString(), i.ToString());
				}

				sender.Api.Messages.Send(new MessagesSendParams()
				{
					RandomId = Environment.TickCount,
					PeerId = eventArgs.PeerId,
					Message = $"сколько будет {firstNum} + {secondNum}?",
					Keyboard = keyboard.Build()

				});
			});

			bot.TemplateManager.Register("матеша", "ну окей, вот тебе матеша");

			bot.TemplateManager.Register(@"\d+$", (sender, eventArgs) =>
			{
				sender.Logger.LogInformation(eventArgs.Text);
				if (!isBotStarted) return;
				int userAnswer = int.Parse(Regex.Match(eventArgs.Text, @"\d+$").Value);
				var keyboard = new KeyboardBuilder().AddButton("матеша", "1");
				sender.Api.Messages.Send(new MessagesSendParams()
				{
					RandomId = Environment.TickCount,
					PeerId = eventArgs.PeerId,
					Message = (userAnswer == validAnswer)
						? "верный ответ! держи печенюху"
						: $"ответ {userAnswer} невернен! верный ответ был: {validAnswer}, попробуйте еще раз",
					Keyboard = keyboard.Build()

				});
				isBotStarted = false;
			});
			bot.Start();
			bot.Dispose();
			Console.ReadLine();
		}
	}
}
