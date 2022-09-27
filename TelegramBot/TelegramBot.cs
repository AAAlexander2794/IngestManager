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

        public delegate Task EventHandler(object sender, EventArgs args);
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
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message == null) return;

                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "Привет-привет!! " + message.Chat.Id);
                MessageRecived?.Invoke(null, EventArgs.Empty);
            }
        }

        public static async Task SendMessage()
        {
            // Отправка сообщения админу
            await Bot.SendTextMessageAsync(Config.ConfigInfo.AdminChatId, "Отправка админу");
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Debug.WriteLine("Ошибка в Телеграм боте");
        }

        
    }
}
