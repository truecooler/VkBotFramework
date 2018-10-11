# VkBotFramework
Удобная и маленькая библиотека для создания ботов в соц. сети ВКонтакте
## Getting Started
Эти инструкция позволит вам просто и быстро сделать своего бота для вк.
### Prerequisites
На данный момент можно создавать только групповых ботов, а значит вам потребуется AccessToken и GroupId для взаимодействия с группой, а так же выставить необхдимые права доступа боту.

#### Создаем AccessToken
Вы можете создать его в интерфейсе настроек сообщества. Для этого достаточно открыть раздел «Управление сообществом» («Управление страницей», если у Вас публичная страница), выбрать вкладку «Работа с API» и нажать «Создать ключ доступа».
Более подробно можете прочесть тут: %ссылка%
### Example
Использование крайне простое:

```c#
static void Main(string[] args)
	{
  	...
	VkBot bot = new VkBot(settings.AccessToken, settings.GroupId);
	/*подписываемся на событие о входящем сообщении*/
	bot.OnMessageReceived += MessageReceivedTest; 
	
	/*подписываемся на событие об изменении в группе*/
	bot.OnGroupUpdateReceived += UpdateReceivedTest; 
  
	/*регистрируем шаблон {регулярное выражение,ответ бота}*/
	bot.RegisterPhraseTemplate("привет", "на привет всегда отвечаю кусь"); 
	
	/*регистрируем шаблон {регулярное выражение,случайная фраза из списка}*/
	bot.RegisterPhraseTemplate("ты кто", new List<string>() {"меня зовут мишутка","вы о ком","не говори так со мной","а ты кто?"}); 
	
	/*регистрируем шаблон {регулярное выражение, user specified callback}
	bot.RegisterPhraseTemplate("колобок", (msg) =>
	{
		Console.WriteLine($"кто-то написал {msg.Text}, я могу регировать на эту фразу так, как я хочу! system(\"reboot\")");
	});
	/*запускаем бота с блокировкой текущего потока...*/
	bot.Start();
  	...
  }
        
```
К библиотеке прилагается пример, с которым вы можете ознакомиться [тут](https://github.com/truecooler/VkBotFramework/blob/master/VkBotExample/Program.cs)
## Built With

* [VisualStudio](http://visualstudio.com)


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
