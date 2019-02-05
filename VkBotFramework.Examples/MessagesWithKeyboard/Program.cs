using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VkBotFramework;
using VkBotFramework.Examples;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace MessagesWithKeyboard
{

	class PeerContext
	{
		public int ValidAnswer;
	}

	class Program
	{
		static void Main(string[] args)
		{
			ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
			ILogger<VkBot> logger = loggerFactory.CreateLogger<VkBot>();

			ExampleSettings settings = ExampleSettings.TryToLoad(logger);

			VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl, logger);

			//для поддержки сразу нескольких диалогов
			var participatingPeers = new Dictionary<long, PeerContext>();

			bot.TemplateManager.Register("\\+матеша", "ну окей, вот тебе матеша");

			bot.TemplateManager.Register("\\+матеша", (sender, eventArgs) =>
			{
				var rand = new Random();
				var firstNum = rand.Next(1, 100);
				var secondNum = rand.Next(1, 100);
				var validAnswer = firstNum + secondNum;
				var peerContext = new PeerContext(){ValidAnswer = validAnswer };
				participatingPeers.Add(eventArgs.PeerId.Value, peerContext);

				int buttonsCount = 4;

				int validButtonIndex = rand.Next(buttonsCount);
				var keyboard = new KeyboardBuilder();
				for (int i = 0; i < buttonsCount; i++)
				{
					if (i == validButtonIndex)
						keyboard.AddButton((validAnswer).ToString(), "");
					else
						keyboard.AddButton(rand.Next(200).ToString(), "");
				}

				sender.Api.Messages.Send(new MessagesSendParams()
				{
					RandomId = Math.Abs(Environment.TickCount),
					PeerId = eventArgs.PeerId,
					Message = $"сколько будет {firstNum} + {secondNum}?",
					Keyboard = keyboard.Build()

				});
			});


			bot.TemplateManager.Register("-матеша", "ну окей, теперь не будет у вас этой кнопки",new KeyboardBuilder().SetOneTime().AddButton("пока","").Build());
			bot.TemplateManager.Register(@"\d+$", (sender, eventArgs) =>
			{
				PeerContext peerContext;
				if (!participatingPeers.TryGetValue(eventArgs.PeerId.Value,out peerContext)) return;

				int userAnswer = int.Parse(Regex.Match(eventArgs.Text, @"\d+$").Value);

				var keyboard = new KeyboardBuilder()
					.AddButton("+матеша", "",KeyboardButtonColor.Positive)
					.AddButton("-матеша","",KeyboardButtonColor.Negative);

				sender.Api.Messages.Send(new MessagesSendParams()
				{
					RandomId = Math.Abs(Environment.TickCount),
					PeerId = eventArgs.PeerId,
					Message = (userAnswer == peerContext.ValidAnswer)
						? "верный ответ! держи печенюху"
						: $"ответ {userAnswer} невернен! верный ответ был: {peerContext.ValidAnswer}, попробуйте еще раз",
					Keyboard = keyboard.Build()

				});
				participatingPeers.Remove(eventArgs.PeerId.Value);
			});
			bot.Start();
			bot.Dispose();
			Console.ReadLine();
		}
	}
}
