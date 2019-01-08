using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace VkBotFramework.Examples
{
	public class ExampleSettings
	{
		public static void CreateDefaults()
		{
			ExampleSettings settings = new ExampleSettings();
			settings.AccessToken = "put_your_vk_group_token_here";
			settings.GroupUrl = "put_your_group_url_here";
			File.WriteAllText(ExampleSettings.Filename, JsonConvert.SerializeObject(settings));
		}
		public static ExampleSettings Load()
		{
			if (!File.Exists(ExampleSettings.Filename))
			{
				return null;
			}
			return JsonConvert.DeserializeObject<ExampleSettings>(File.ReadAllText(ExampleSettings.Filename));
		}

		public static ExampleSettings TryToLoad(ILogger<VkBot> _Logger)
		{
			ExampleSettings settings = null;

			if ((settings = ExampleSettings.Load()) == null)
			{
				_Logger.LogWarning(@"Файл с настройками ExampleSettings.json не найден в VkBotFramework.Examples\ExampleSettings\bin. Будет создан файл настроек по-умолчанию в указанном месте.");
				_Logger.LogWarning("Занесите в него корректные параметры для вашего бота и запустите пример снова");
				ExampleSettings.CreateDefaults();
				Console.ReadLine();
				Environment.Exit(0);
			}
			_Logger.LogInformation("Настройки загружены.");
			return settings;
		}

		public static string Filename
		{
			get
			{
				return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
				       + @"/../../../../ExampleSettings/bin/ExampleSettings.json";
			}
		}

		public string AccessToken;
		public string GroupUrl;
	}
}
