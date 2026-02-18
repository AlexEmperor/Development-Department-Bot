using DevelopmentLaboratoryBot;
using Telegram.Bot;



using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(Constants.TOKEN, cancellationToken: cts.Token);
var _messageHandler = new MessageHandler(bot);
var me = await bot.GetMe();

bot.OnError += _messageHandler.OnError;
bot.OnMessage += _messageHandler.OnMessage;
bot.OnUpdate += _messageHandler.OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

