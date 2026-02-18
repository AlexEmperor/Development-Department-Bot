using DevelopmentLaboratoryBot;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var userStates = new Dictionary<long, string>();
var formData = new Dictionary<long, (string Name, string Email, string TaskDescription)>();
var calcData = new Dictionary<long, (string Type, string Complexity, string Duration)>();

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(Constants.TOKEN, cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

// ======= Словари для хранения состояния пользователей (для формы и калькулятора) =======


// =================== Ошибки ===================
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception); // просто выводим в консоль
}

// =================== Главное меню ===================
InlineKeyboardMarkup MainMenuKeyboard() =>
    new InlineKeyboardMarkup(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("🧪 Проекты", "projects") },
        new[] { InlineKeyboardButton.WithCallbackData("⚙️ Услуги", "services") },
        new[] { InlineKeyboardButton.WithCallbackData("📞 Контакты", "contacts") },
        new[] { InlineKeyboardButton.WithCallbackData("💬 Написать специалисту", "write_to_human") },
        new[] { InlineKeyboardButton.WithCallbackData("📝 Оставить заявку", "online_form") },
        new[] { InlineKeyboardButton.WithCallbackData("📰 Новости", "news") },
        new[] { InlineKeyboardButton.WithCallbackData("📊 Оценка проекта", "project_calc") },
        new[] { InlineKeyboardButton.WithCallbackData("🔔 Подписка на обновления", "subscribe") }
    });

// =================== Кнопка возврата ===================
InlineKeyboardMarkup ReturnKeyboard() =>
    new InlineKeyboardMarkup(
        InlineKeyboardButton.WithCallbackData("⬅ Вернуться в меню", "main_menu")
    );

// =================== Обработка сообщений ===================
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == null)
    {
        return;
    }

    var chatId = msg.Chat.Id;

    // ======== Онлайн-заявка ========
    if (userStates.ContainsKey(chatId) && userStates[chatId].StartsWith("form_"))
    {
        switch (userStates[chatId])
        {
            case "form_name":
                formData[chatId] = (msg.Text, "", "");
                userStates[chatId] = "form_email";
                await bot.SendMessage(chatId, "Введите ваш E-mail:");
                break;
            case "form_email":
                var oldForm = formData[chatId];
                formData[chatId] = (oldForm.Name, msg.Text, "");
                userStates[chatId] = "form_task";
                await bot.SendMessage(chatId, "Коротко опишите задачу:");
                break;
            case "form_task":
                var data = formData[chatId];
                formData[chatId] = (data.Name, data.Email, msg.Text);

                var adminChatId = -5123579887; // <- сюда свой Telegram ID
                await bot.SendMessage(adminChatId,
                    $"Новая заявка:\n\nИмя: {data.Name}\nEmail: {data.Email}\nОписание задачи: {msg.Text}");

                await bot.SendMessage(chatId, "✅ Заявка отправлена! Спасибо!");
                userStates.Remove(chatId);
                formData.Remove(chatId);
                break;
        }
        return;
    }

    // ======== Калькулятор проекта ========
    if (userStates.ContainsKey(chatId) && userStates[chatId].StartsWith("calc_"))
    {
        switch (userStates[chatId])
        {
            case "calc_type":
                calcData[chatId] = (msg.Text, "", "");
                userStates[chatId] = "calc_complexity";
                await bot.SendMessage(chatId, "Введите сложность проекта (низкая/средняя/высокая):");
                break;
            case "calc_complexity":
                var oldCalc = calcData[chatId];
                calcData[chatId] = (oldCalc.Type, msg.Text, "");
                userStates[chatId] = "calc_duration";
                await bot.SendMessage(chatId, "Введите предполагаемую продолжительность (в неделях):");
                break;
            case "calc_duration":
                var calc = calcData[chatId];
                calcData[chatId] = (calc.Type, calc.Complexity, msg.Text);

                // Простейшая оценка (для примера)
                var estCost = calc.Complexity.ToLower() switch
                {
                    "низкая" => 50000,
                    "средняя" => 120000,
                    "высокая" => 250000,
                    _ => 100000
                };

                await bot.SendMessage(chatId,
                    $"📊 Оценка проекта:\nТип: {calc.Type}\nСложность: {calc.Complexity}\n" +
                    $"Сроки: {calc.Duration} недель\nПримерная стоимость: {estCost} руб.");

                userStates.Remove(chatId);
                calcData.Remove(chatId);
                break;
        }
        return;
    }

    // ======== Стартовое сообщение ========
    if (msg.Text.StartsWith("/start"))
    {
        await bot.SendMessage(
            chatId,
            "👋 Привет!\nНа связи лаборатория разработки систем беспроводной связи 📡\n\nЧем могу помочь?",
            replyMarkup: MainMenuKeyboard()
        );
    }
}

// =================== Обработка колбеков ===================
async Task OnUpdate(Update update)
{
    if (update is not { CallbackQuery: { } query })
    {
        return;
    }

    await bot.AnswerCallbackQuery(query.Id);

    switch (query.Data)
    {
        case "main_menu":
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Чем могу помочь?",
                replyMarkup: MainMenuKeyboard()
            );
            break;

        case "projects":
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text:
                "🧪 Наши проекты:\n\n" +
                "1️⃣ Анализатор спектра\nПрограммный комплекс для визуализации и анализа радиочастот.\n\n" +
                "2️⃣ Система радиомониторинга\nПлатформа для удалённого контроля радиосигналов.\n\n" +
                "3️⃣ Программный приёмник SDR\nМодульный приёмник для приёма и обработки цифровых сигналов.",
                replyMarkup: ReturnKeyboard()
            );
            break;

        case "services":
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text:
                "⚙️ Услуги:\n\n" +
                "• Разработка ПО под заказ\n" +
                "• Создание прототипов устройств\n" +
                "• Интеграция оборудования\n" +
                "• Технические консультации\n" +
                "• Сопровождение проектов",
                replyMarkup: ReturnKeyboard()
            );
            break;

        case "contacts":
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text:
                "📞 Контакты:\nТелефон: +7 977 488 9030\nE-mail: alex04120445@mail.ru\nАдрес: 125183, г. Москва, Проспект Черепановых, д. 54\n" +
                "Контактное лицо: Тарасов Игорь Анатольевич\nВремя работы: пн-пт: 9:00 - 18:00",
                replyMarkup: ReturnKeyboard()
            );
            break;

        case "write_to_human":
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text: "💬 Переход к специалисту: https://t.me/YgorGrupStar",
                replyMarkup: ReturnKeyboard()
            );
            break;

        case "online_form":
            userStates[query.Message!.Chat.Id] = "form_name";
            await bot.EditMessageText(
                chatId: query.Message.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Введите ваше имя:",
                replyMarkup: ReturnKeyboard()
            );
            break;

        case "news":
            await bot.EditMessageText(
                chatId: query.Message.Chat.Id,
                messageId: query.Message.MessageId,
                text:
                "📰 Новости лаборатории:\n\n" +
                "1️⃣ Выпущен новый анализатор спектра\n" +
                "2️⃣ Проведён успешный тест системы радиомониторинга\n" +
                "3️⃣ Опубликован отчёт по SDR-приёмнику",
                replyMarkup: ReturnKeyboard()
            );
            break;

        case "project_calc":
            userStates[query.Message!.Chat.Id] = "calc_type";
            await bot.EditMessageText(
                chatId: query.Message.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Введите тип устройства для оценки:",
                replyMarkup: ReturnKeyboard()
            );
            break;

        case "subscribe":
            await bot.EditMessageText(
                chatId: query.Message.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Вы подписались на обновления лаборатории. 🛎️",
                replyMarkup: ReturnKeyboard()
            );
            break;
    }
}
