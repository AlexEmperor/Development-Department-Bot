using Telegram.Bot.Types.ReplyMarkups;

namespace DevelopmentLaboratoryBot
{
    public static class ButtonHandler
    {
        // =================== Главное меню ===================
        public static InlineKeyboardMarkup MainMenuKeyboard() =>
            new InlineKeyboardMarkup(new[]
            {
        new[] { InlineKeyboardButton.WithCallbackData("🧪 Проекты", "projects") },
        new[] { InlineKeyboardButton.WithCallbackData("⚙️ Услуги", "services") },
        new[] { InlineKeyboardButton.WithCallbackData("📞 Контакты", "contacts") },
        new[] { InlineKeyboardButton.WithCallbackData("💬 Написать специалисту", "write_to_human") },
        new[] { InlineKeyboardButton.WithCallbackData("📝 Оставить заявку", "online_form") },
        new[] { InlineKeyboardButton.WithCallbackData("📰 Новости", "news") }
        // new[] { InlineKeyboardButton.WithCallbackData("📊 Оценка проекта", "project_calc") }
            });

        // =================== Кнопка возврата ===================
        public static InlineKeyboardMarkup ReturnKeyboard() =>
            new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("⬅ Вернуться в меню", "main_menu")
            );
    }
}
