using IngestManager.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace IngestManager.Models.TelegramBot
{
    internal static class TelegramBot
    {
        static ITelegramBotClient Bot = new TelegramBotClient(Config.ConfigInfo.Hash);

        public delegate Task EventHandler(object? sender, EventArgs args);
        public static event EventHandler? MessageRecived;

        /// <summary>
        /// Запускает бота
        /// </summary>
        public static void StartBot()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            Bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Обработка именно сообщений
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message == null || message.Text == null) return;
                // Начальное приветствие
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Вы подключены к боту Инжеста.");
                    return;
                }
                // Аргументы для соб
                var telegramEventArgs = new TelegramEventArgs(message);
                //
                await botClient.SendTextMessageAsync(message.Chat, "Сообщение получено, запрос обрабатывается.");
                // Объект не отправляем, так как sender не экземпляр - он статический
                MessageRecived?.Invoke(null, telegramEventArgs);
            }
        }

        public static async Task SendMessageAsync(long chatId, string message)
        {
            _ = await Bot.SendTextMessageAsync(chatId, message);
        }

        /// <summary>
        /// Отправка сообщения админу бота
        /// </summary>
        /// <returns></returns>
        public static async Task SendMessageToAdminAsync(string message)
        {
            // Оператор удаления, чтобы не дожидаться отправки сообщения
            _ = await Bot.SendTextMessageAsync(Config.ConfigInfo.AdminChatId, message);
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Debug.WriteLine("Ошибка в Телеграм боте");
        }

        
    }
}
