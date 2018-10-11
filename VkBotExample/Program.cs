using System;
using System.IO;
using System.Collections.Generic;
using VkBotFramework;
using VkNet.Enums.SafetyEnums;
using Newtonsoft.Json;
namespace VkBotExample
{
    class Program
    {
		class Settings
		{
			public static void CreateDefaults()
			{
				Settings settings = new Settings();
				settings.AccessToken = "put_your_vk_group_token_here";
				settings.GroupId = 0;
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
			public ulong GroupId;
		}


		static void MessageReceivedTest(object sender, VkBot.MessageReceivedEventArgs args)
		{
			Console.WriteLine($"MessageReceivedTest Works!: {args.message.PeerId}: {args.message.Text}");
		}
		static void UpdateReceivedTest(object sender, VkBot.GroupUpdateReceivedEventArgs args)
		{
			if (args.update.Type == GroupUpdateType.MessageReply)
			{
				Console.WriteLine($"UpdateReceivedTest Works! intercept output message: {args.update.Message.PeerId}: {args.update.Message.Text}");
			}
		}
        static void Main(string[] args)
        {
			Settings settings = null;
			if ((settings = Settings.Load()) == null)
			{
				Console.WriteLine("Файл с настройками не найден рядом с бинарником. Будет создан файл настроек по-умолчанию.");
				Console.WriteLine("Занесите в него корректные параметры для вашего бота и запустите пример снова");
				Settings.CreateDefaults();
				Console.ReadLine();
				return;
			}
			Console.WriteLine("Настройки загружены.");
			VkBot bot = new VkBot(settings.AccessToken, settings.GroupId);
			bot.OnMessageReceived += MessageReceivedTest;
			bot.OnGroupUpdateReceived += UpdateReceivedTest;
			bot.RegisterPhraseTemplate("привет", "на привет всегда отвечаю кусь");
			bot.RegisterPhraseTemplate("ты кто", new List<string>() {"кавоо бля","вы о ком","не говори так","а ты кто?" }  );
			bot.RegisterPhraseTemplate("колобок", (msg) =>
			{
				Console.WriteLine($"кто-то написал {msg.Text}, я могу регировать на эту фразу так, как я хочу! system(\"reboot\")");
			});
			bot.Start();
			Console.ReadLine();
        }
    }
}
