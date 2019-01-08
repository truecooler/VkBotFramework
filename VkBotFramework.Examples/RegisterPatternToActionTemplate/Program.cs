using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VkNet.Model.RequestParams;
using VkBotFramework;
using VkBotFramework.Examples;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;

namespace RegisterPatternToActionTemplate
{
    class Program
    {
        static void Main(string[] args)
        {
			ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
			ILogger<VkBot> logger = loggerFactory.CreateLogger<VkBot>();

			ExampleSettings settings = ExampleSettings.TryToLoad(logger);

			VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl, logger);
			bot.TemplateManager.Register("привет", "на привет всегда отвечаю кусь");
			bot.TemplateManager.Register("ты кто", new List<string>() { "меня зовут мишутка", "вы о ком", "не говори так", "а ты кто?" });

			bot.TemplateManager.Register("^[0-9]+$", "ого, я определил, что вы прислали мне число!");

			bot.TemplateManager.Register("колобок", (sender, msg) =>
			{
				logger.LogInformation($"кто-то написал {msg.Text}, я могу регировать на эту фразу так, как я хочу! system(\"reboot\")");
			});

			bot.TemplateManager.Register("квадр.*[0-9]+", (sender, msg) =>
			{

				logger.LogInformation($"кто-то написал '{msg.Text}', пора вычислить квадрат числа в сообщении!");

				int num = int.Parse(Regex.Match(msg.Text, "[0-9]+").Value);

				sender.Api.Messages.Send(new MessagesSendParams()
				{
					RandomId = Environment.TickCount,
					PeerId = msg.PeerId,
					Message = $"квадрат числа {num} равен {num * num}"
				});
			});


			var keyboard = new KeyboardBuilder().SetOneTime()
				.AddButton("лол", "кек", KeyboardButtonColor.Positive, "type").Build();
			bot.TemplateManager.Register("кнопка","лови кнопку", keyboard);
			bot.Start();
			bot.Dispose();
			Console.ReadLine();
		}
    }
}
