using DevelopmentLaboratoryBot;
using Telegram.Bot;
using Telegram.Bot.Polling;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(Constants.TOKEN, cancellationToken: cts.Token);
var _messageHandler = new MessageHandler(bot);
var me = await bot.GetMe();

bot.OnError += OnError;
bot.OnMessage += _messageHandler.OnMessage;
bot.OnUpdate += _messageHandler.OnUpdate;

async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception); // просто выводим в консоль
}

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

