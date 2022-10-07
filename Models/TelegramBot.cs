using IngestManager.Entities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace IngestManager.Models;

internal static class TelegramBot
{
    static ITelegramBotClient Bot = new TelegramBotClient(Config.ConfigInfo.Hash);

    /// <summary>
    /// Токен отмены. Нужен в некоторых методах.
    /// </summary>
    static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

    public delegate Task EventHandler(object? sender, TelegramEventArgs args);
    public static event EventHandler? OnUpdate;

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

    public static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Объект не отправляем, так как sender не экземпляр - он статический
        OnUpdate?.Invoke(null, new TelegramEventArgs(update));
        return Task.CompletedTask;
    }

    public static async Task<Message> SendMessageAsync(long chatId, string text)
    {
        var message = await Bot.SendTextMessageAsync(chatId, text);
        return message;
    }

    public static async Task<Message> SendMessageAsync(long chatId, string text, InlineKeyboardButton[] buttons)
    {
        InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);
        var message = await Bot.SendTextMessageAsync(chatId, text,
            replyMarkup: inline,
            cancellationToken: CancellationTokenSource.Token);
        return message;
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

    /// <summary>
    /// Ответ на запрос <see cref="CallbackQuery"/>.
    /// </summary>
    /// <param name="callbackQueryId"></param>
    /// <returns></returns>
    /// <remarks>
    /// Его нужно отправлять, чтобы Телеграм получил сообщение о том, что запрос обработан.
    /// </remarks>
    public static async Task AnswerCallbackQueryAsync(string callbackQueryId)
    {
        // Отвечаю на запрос, вызванный нажатием кнопки (иначе на кнопке висели бы часики как на неотправленном сообщении)
        await Bot.AnswerCallbackQueryAsync(callbackQueryId, null);
    }

    /// <summary>
    /// Метод обработки ошибок
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Некоторые действия
        Debug.WriteLine("Ошибка в Телеграм боте");
    }

    public static async Task EditMessageAsync(long chatId, int messageId, string text)
    {
        await Bot.EditMessageTextAsync(chatId, messageId, text);
    }

    public static async Task EditMessageAsync(long chatId, int messageId, string text, InlineKeyboardButton[] buttons)
    {
        InlineKeyboardMarkup inline = new InlineKeyboardMarkup(buttons);
        await Bot.EditMessageTextAsync(chatId, messageId, text, 
            replyMarkup: inline,
            cancellationToken: CancellationTokenSource.Token);
    }

}
