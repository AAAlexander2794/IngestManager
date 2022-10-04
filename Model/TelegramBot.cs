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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace IngestManager.Models.TelegramBot
{
    internal static class TelegramBot
    {
        static ITelegramBotClient Bot = new TelegramBotClient(Config.ConfigInfo.Hash);

        /// <summary>
        /// Токен отмены. Нужен в некоторых методах.
        /// </summary>
        static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public delegate Task EventHandler(object? sender, EventArgs args);
        public static event EventHandler? MessageRecived;

        /// <summary>
        /// Запускает бота
        /// </summary>
        public static void StartBot()
        {
            var cancellationToken = CancellationTokenSource.Token;
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
            //await SendMessageToAdminAsync("получен ответ " + update.Type.ToString());
            if (update.Type == UpdateType.CallbackQuery)
            {
                await OnCallbackQuery(update, new EventArgs());
                // Отвечаю на запрос, вызванный нажатием кнопки (иначе на кнопке висели бы часики как на неотправленном сообщении)
                await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, null);
            }
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

        public static async Task SendMessageWithButtonsAsync(long chatId)
        {
            //creating inline keyboard
            InlineKeyboardButton accept = new InlineKeyboardButton("");
            InlineKeyboardButton reject = new InlineKeyboardButton("");
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
   accept, reject
            };
            InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);

            //giving inline buttons text and callback data
            accept.Text = "accept";
            reject.Text = "reject";
            accept.CallbackData = "accept";
            reject.CallbackData = "reject";

            Message sentMessage = await Bot.SendTextMessageAsync(
                chatId: chatId,
                text: "choose pls",
                replyMarkup: inline,
                cancellationToken: CancellationTokenSource.Token);
        }

        private static async Task OnCallbackQuery(Update update, EventArgs e)
        {

            // Send the message to any one you want
            ChatId chatId = Config.ConfigInfo.AdminChatId;
            await Bot.SendTextMessageAsync(chatId, "Вы нажали на кнопку: " + update.CallbackQuery.Data.ToString());
        }

        /// <summary>
        /// Метод обработки ошибок
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Debug.WriteLine("Ошибка в Телеграм боте");
        }

        
    }
}
