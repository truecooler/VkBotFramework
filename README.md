# VkBotFramework
![VkBotFramework Logo](https://github.com/truecooler/VkBotFramework/raw/master/vkbotframework.png)

Удобная, маленькая и кроссплатформенная библиотека для создания ботов в соц. сети ВКонтакте
## Getting Started
Эти инструкция позволит вам просто и быстро сделать своего бота для вк.

### Install
Установите Nuget пакет в ваш проект:
**Package Manager**
``` powershell
PM> Install-Package VkBotFramework
```
**.NET CLI**
``` bash
> dotnet add package VkBotFramework
```
**Visual Studio Nuget Manager**
```
Проект -> Свойства -> Управление пакетами Nuget -> Обзор -> Поиск -> VkBotFramework -> Установить
```

### Prerequisites
На данный момент можно создавать только групповых ботов, а значит вам потребуется AccessToken и GroupUrl для взаимодействия с группой, а так же выставить необхдимые права доступа боту.

#### Начальная настройка
##### AccessToken
- Вы можете создать его в интерфейсе настроек сообщества. Для этого достаточно открыть раздел «Управление сообществом» («Управление страницей», если у Вас публичная страница), выбрать вкладку «Работа с API» и нажать «Создать ключ доступа». Так же есть и другие способы получить токен, ознакомиться можно [тут](https://vk.com/dev/access_token).
##### Enable Pong Poll Api
- Так же необходимо зайти во вкладку Long Poll Api, перевести состояние Long Poll Api во "включен",а так же во вкладке "типы событий" выбрать нужные события, которые будут приходить боту.
##### Что такое GroupUrl?
- Это ссылка на вашу группу, которую можно взять прямо из адресной строки браузера. Библиотека сама определит id группы для своих нужд.
##### Wiki
- Более подробно о настройке и использовании бота можно прочесть в [wiki](https://github.com/truecooler/VkBotFramework/wiki).
## Example
Использование библиотеки крайне простое:

```c#
VkBot bot = new VkBot(settings.AccessToken, settings.GroupUrl);
/*подписываемся на событие о входящем сообщении, в которое передается экземпляр сообщения*/
bot.OnMessageReceived += MessageReceivedTest; 

/*подписываемся на событие об изменении в группе, в которое передается экземпляр события в группе*/
bot.OnGroupUpdateReceived += UpdateReceivedTest; 

/*регистрируем шаблон {регулярное выражение,ответ бота}*/
bot.TemplateManager.Register("привет", "на привет всегда отвечаю кусь"); 
bot.TemplateManager.Register("^[0-9]+$", "ого, я определил, что вы прислали мне число!");

/*регистрируем шаблон {регулярное выражение,случайная фраза из списка}*/
bot.TemplateManager.Register("ты кто", new List<string>() {"меня зовут мишутка","вы о ком","не говори так со мной","а ты кто?"}); 

/*регистрируем шаблон {регулярное выражение, user specified callback}*/
bot.TemplateManager.Register("колобок", (msg) =>
{
	Console.WriteLine($"кто-то написал {msg.Text}, я могу регировать на эту фразу так, как я хочу! system(\"reboot\")");
});

/*запускаем бота синхронно...*/
bot.Start();
        
```
С наглядными примерами можно ознакомиться [тут](https://github.com/truecooler/VkBotFramework/tree/master/VkBotFramework.Examples)

## TODOs
- [x] Сделать подписки на события о приходе сообщения/обновления в группе
- [x] Сделать функционал регистрации шаблона {регулярная фраза,ответ/список ответов/user callback}
- [x] Сделать асинхронный Start
- [x] Добавить функционал кнопок
- [x] Сделать логгирование
- [ ] Сделать тесты
- [ ] Сделать библиотеку пригодной для создания бота пользователя(без группы)

## Зависимости

* [VkNet](https://github.com/vknet/vk) - взаимодействие с Api ВКонтакте.

## Built With

* [VisualStudio](http://visualstudio.com)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
