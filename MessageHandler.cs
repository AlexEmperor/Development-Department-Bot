using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevelopmentLaboratoryBot
{
    public class MessageHandler
    {
        // ======= Словари для хранения состояния пользователей (для формы и калькулятора) =======
        private Dictionary<long, string> userStates = [];
        private Dictionary<long, (string Name, string Email, string TaskDescription)> formData = [];
        //private Dictionary<long, (string Type, string Complexity, string Duration)> calcData = [];

        private TelegramBotClient bot;

        public MessageHandler(TelegramBotClient client)
        {
            bot = client;
        }

        // =================== Обработка сообщений ===================
        public async Task OnMessage(Message msg, UpdateType type)
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

                        var adminChatId = -5123579887; // сюда свой Telegram ID группы/чата
                        await bot.SendMessage(adminChatId,
                            $"Новая заявка:\n\nИмя: {data.Name}\nEmail: {data.Email}\nОписание задачи: {msg.Text}");

                        // Отправляем пользователю сообщение о завершении
                        await bot.SendMessage(chatId, "✅ Заявка отправлена! Спасибо!");

                        await bot.SendMessage(chatId,
                            "Вернуться в меню:",
                            replyMarkup: ButtonHandler.ReturnKeyboard()
                        );

                        userStates.Remove(chatId);
                        formData.Remove(chatId);
                        break;

                }
                return;
            }

            // ======== Калькулятор проекта ========
            /*if (userStates.ContainsKey(chatId) && userStates[chatId].StartsWith("calc_"))
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

                        await bot.SendMessage(chatId,
                            "Вернуться в меню:",
                            replyMarkup: ReturnKeyboard()
                        );

                        userStates.Remove(chatId);
                        calcData.Remove(chatId);
                        break;
                }
                return;
            }
            */

            // ======== Стартовое сообщение ========
            if (msg.Text.StartsWith("/start"))
            {
                await bot.SendMessage(
                    chatId,
                    "👋 Привет!\nНа связи лаборатория разработки систем беспроводной связи 📡\n\nЧем могу помочь?",
                    replyMarkup: ButtonHandler.MainMenuKeyboard()
                );
            }
        }

        // =================== Обработка колбеков ===================
        public async Task OnUpdate(Update update)
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
                        replyMarkup: ButtonHandler.MainMenuKeyboard()
                    );
                    break;

                case "projects":
                    await bot.EditMessageText(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        "1️⃣ АК «SDR».\nУстройство сканирования частотного спектра с возможностью указания на карте источников различных типов сигналов.\n\n" +
                        "2️⃣ Аппаратный модуль VVizor.\nУстройство фиксирования видео передатчиков.\n\n" +
                        "3️⃣ МЭМС.\nУстройство позиционирования.\n\n" +
                        "4️⃣ Плата ретранслятора с программным обеспечением автоматизированного тестирования выпускаемых изделий.\n\n" +
                        "5️⃣ Радиосканер.\nДвухканальное устройство сканирования частотного спектра.\n\n" +
                        "6️⃣ Bluetooth-маяки.\n\n" +
                        "7️⃣ Радиомаяки.\n\n",

                        replyMarkup: ButtonHandler.ReturnKeyboard()
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
                        "• Консультационные услуги\n" +
                        "• Поставка готовых изделий\n" +
                        "• Гарантийное обслуживание\n" +
                        "• Сопровождение проектов",
                        replyMarkup: ButtonHandler.ReturnKeyboard()
                    );
                    break;

                case "contacts":
                    await bot.EditMessageText(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        "📞 Контакты:\nТелефон: +7(977)488-90-30\nE-mail: alex04120445@mail.ru\nТелеграм: @YgorGrupStar\nАдрес: 125183, г. Москва, Проспект Черепановых, д. 54\n" +
                        "Контактное лицо: Тарасов Игорь Анатольевич\nВремя работы: пн-пт: 9:00 - 18:00",
                        replyMarkup: ButtonHandler.ReturnKeyboard()
                    );
                    break;

                case "write_to_human":
                    await bot.EditMessageText(
                        chatId: query.Message!.Chat.Id,
                        messageId: query.Message.MessageId,
                        text: "💬 Переход к специалисту: https://t.me/YgorGrupStar",
                        replyMarkup: ButtonHandler.ReturnKeyboard()
                    );
                    break;

                case "online_form":
                    userStates[query.Message!.Chat.Id] = "form_name";
                    await bot.EditMessageText(
                        chatId: query.Message.Chat.Id,
                        messageId: query.Message.MessageId,
                        text: "Введите ваше имя:",
                        replyMarkup: ButtonHandler.ReturnKeyboard()
                    );
                    break;

                case "news":
                    await bot.EditMessageText(
                        chatId: query.Message.Chat.Id,
                        messageId: query.Message.MessageId,
                        text:
                        "📰 Новости лаборатории:\n\n" +
                        "1️⃣ Проведена проверка изделия AK SDR в полевых условиях на полигоне в г. Калуга\n" +
                        "2️⃣ Отладка устройства VVizor.\n" +
                        "3️⃣ Выпущена очередная партия МЭМС",
                        replyMarkup: ButtonHandler.ReturnKeyboard()
                    );
                    break;

                    /*case "project_calc":
                        userStates[query.Message!.Chat.Id] = "calc_type";
                        await bot.EditMessageText(
                            chatId: query.Message.Chat.Id,
                            messageId: query.Message.MessageId,
                            text: "Введите тип устройства для оценки:",
                            replyMarkup: ReturnKeyboard()
                        );
                        break;*/
            }
        }
    }
}
