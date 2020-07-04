using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using VkBotFramework;
using VkBotFramework.Examples;
using VkBotFramework.Models;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace MessagesWithKeyboard
{
	class Program
	{
		private static string MateshaSubscribeCommand = "\\+матеша";
		private static string MateshaUnsubscribeCommand = "\\-матеша";
		private static string MateshaNumberCommand = @"\d+$";

		static void MateshaNumberHandler(VkBot sender, MessageReceivedEventArgs args)
		{
			int userAnswer = int.Parse(Regex.Match(args.Message.Text, MateshaNumberCommand).Value);
			int validAnswer = args.PeerContext.Vars["validAnswer"];

			var keyboard = new KeyboardBuilder()
				.AddButton("+матеша", "", KeyboardButtonColor.Positive)
				.AddButton("-матеша", "", KeyboardButtonColor.Negative);

			sender.TemplateManager.Unregister(MateshaNumberCommand, args.Message.PeerId.Value);
			args.PeerContext.Vars.Remove("validAnswer");
			sender.Api.Messages.Send(new MessagesSendParams()
			{
				RandomId = Math.Abs(Environment.TickCount),
				PeerId = args.Message.PeerId,
				Message = (userAnswer == validAnswer)
					? "верный ответ! держи печенюху"
					: $"ответ {userAnswer} невернен! верный ответ был: {validAnswer}, попробуйте еще раз",
				Keyboard = keyboard.Build()
			});
		}

		static void MateshaHandler(VkBot sender, MessageReceivedEventArgs args)
		{
			//правильный ответ уже установлен в диалоге, значит бот ожидает ответа в диалоге
			if (args.PeerContext.Vars.ContainsKey("validAnswer")) return;

			var message = args.Message;
			var rand = new Random();
			var firstNum = rand.Next(1, 100);
			var secondNum = rand.Next(1, 100);
			var validAnswer = firstNum + secondNum;

			//устанавливаем правильный ответ в контекст диалога
			args.PeerContext.Vars["validAnswer"] = validAnswer;

			//регистрируем новый обработчик для этого диалога, который будет чувствителен к числам
			sender.TemplateManager.Register(
				new RegexToActionTemplate(MateshaNumberCommand, MateshaNumberHandler, peerId: message.PeerId.Value)
			);

			//рисуем кнопочки
			int buttonsCount = 10;
			int maxButtonsCountInLine = 4;
			int validButtonIndex = rand.Next(buttonsCount);
			var keyboard = new KeyboardBuilder();
			for (int i = 0; i < buttonsCount; i++)
			{
				if (i == validButtonIndex)
					keyboard.AddButton((validAnswer).ToString(), "", KeyboardButtonColor.Primary);
				else
					keyboard.AddButton(rand.Next(200).ToString(), "", KeyboardButtonColor.Primary);

				if ((i + 1) % maxButtonsCountInLine == 0)
					keyboard.AddLine();
			}

			sender.Api.Messages.Send(new MessagesSendParams()
			{
				RandomId = Math.Abs(Environment.TickCount),
				PeerId = args.Message.PeerId,
				Message = $"сколько будет {firstNum} + {secondNum}?",
				Keyboard = keyboard.Build()
			});
		}

		static void Main(string[] args)
		{
			ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
			ILogger<VkBot> logger = loggerFactory.CreateLogger<VkBot>();

			ExampleSettings settings = ExampleSettings.TryToLoad(logger);

			VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl, logger);


			//регистрируем текстовый ответ на "+матеша"
			bot.TemplateManager.Register(new RegexToActionTemplate(MateshaSubscribeCommand,
				"ну окей, вот тебе матеша"));

			//регистрируем обработчик-функцию на "+матеша"
			bot.TemplateManager.Register(new RegexToActionTemplate(MateshaSubscribeCommand, MateshaHandler));

			//важно: отсутствие PeerId в конструкторе RegexToActionTemplate означает 

			// способ удаления клавиатуры будет добавлен в следующем обновлении проекта VkNet
			//https://github.com/vknet/vk/pull/780

			bot.TemplateManager.Register(new RegexToActionTemplate(MateshaUnsubscribeCommand,
				"ну окей, теперь не будет у вас этой кнопки",
				new KeyboardBuilder().SetOneTime().AddButton("пока", "").Build()));

			bot.Start();
			bot.Dispose();
			Console.ReadLine();
		}
	}
}