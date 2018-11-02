using System;
using System.IO;
using System.Collections.Generic;
using VkBotFramework;
using VkNet.Enums.SafetyEnums;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.DependencyInjection;
using VkNet;

namespace VkBotExample
{
    class Program
    {
		static ILogger<VkBot> Logger;
		static ILoggerFactory LoggerFactory;
		class Settings
		{
			public static void CreateDefaults()
			{
				Settings settings = new Settings();
				settings.AccessToken = "put_your_vk_group_token_here";
				settings.GroupUrl = "put_your_group_url_here";
				File.WriteAllText(Settings.Filename, JsonConvert.SerializeObject(settings));
			}
			public static Settings Load()
			{
				if (!File.Exists(Settings.Filename))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Settings.Filename));
			}

			public static string Filename = "./VkBotSettings.json";
			public string AccessToken;
			public string GroupUrl;
		}


		static void MessageReceivedTest(object sender, VkBot.MessageReceivedEventArgs args)
		{
			Logger.LogInformation($"MessageReceivedTest Works!: {args.message.PeerId}: {args.message.Text}");
		}
		static void UpdateReceivedTest(object sender, VkBot.GroupUpdateReceivedEventArgs args)
		{
			if (args.update.Type == GroupUpdateType.MessageReply)
			{
				Logger.LogInformation($"UpdateReceivedTest Works! intercept output message: {args.update.Message.PeerId}: {args.update.Message.Text}");
			}
		}

		static void Main(string[] args)
		{
			LoggerFactory = new LoggerFactory().AddConsole();
			Logger = LoggerFactory.CreateLogger<VkBot>();
			
			Settings settings = null;
			if ((settings = Settings.Load()) == null)
			{
				Logger.LogWarning("Файл с настройками не найден рядом с бинарником. Будет создан файл настроек по-умолчанию.");
				Logger.LogWarning("Занесите в него корректные параметры для вашего бота и запустите пример снова");
				Settings.CreateDefaults();
				Console.ReadLine();
				return;
			}

			Logger.LogInformation("Настройки загружены.");

			VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl, Logger);

			bot.OnMessageReceived += MessageReceivedTest;
			bot.OnGroupUpdateReceived += UpdateReceivedTest;
			bot.RegisterPhraseTemplate("привет", "на привет всегда отвечаю кусь");
			bot.RegisterPhraseTemplate("ты кто", new List<string>() {"меня зовут мишутка","вы о ком","не говори так","а ты кто?" }  );
			bot.RegisterPhraseTemplate("колобок", (msg) =>
			{
				Logger.LogInformation($"кто-то написал {msg.Text}, я могу регировать на эту фразу так, как я хочу! system(\"reboot\")");
			});
			bot.StartAsync().GetAwaiter().GetResult();
			Console.ReadLine();
        }
    }
}
