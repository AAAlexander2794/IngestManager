using IngestManager.Entities;
using IngestManager.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace IngestManager.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Класс с основной логикой. Класс обращается к <see cref="Config"/> и <see cref="TelegramBot"/>, 
    /// содержит <see cref="Models.Database"/>, <see cref="FileWatcher"/>.
    /// </remarks>
    public class Controller
    {
        Database Database { get; }

        FileWatcher FileWatcher { get; }

        public Controller(Database database)
        {
            Database = database;
            FileWatcher = new FileWatcher(Config.ConfigInfo.FileWatcherCatalogPath);
            TelegramBot.UpdateRecieved += ProccessUpdate;
            FileWatcher.FileCreated += SendMessageFileCreated;
        }

        /// <summary>
        /// Анализирует полученный от Телеграм-бота апдейт и вызывает необходимые методы обработки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        async Task ProccessUpdate(object? sender, TelegramEventArgs args)
        {
            if (args == null) return;
            var update = args.Update;
            // Если пришло сообщение
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message != null)
            {
                if (update.Message.Text == null) return;
                // Начальное приветствие
                if (update.Message.Text.ToLower() == "/start")
                {
                    await TelegramBot.SendMessageAsync(update.Message.Chat.Id, "Вы подключены к боту Инжеста.");
                    return;
                }
                else if (Regex.Replace(update.Message.Text.ToLower(), @"\s+", "") == "яоператор")
                {
                    var operatorId = update.Message.Chat.Id;
                    Database.CurrentOperatorChatId = operatorId;
                    Config.ConfigInfo.OperatorChatId = operatorId;
                    Config.SaveConfig();
                    await TelegramBot.SendMessageAsync(update.Message.Chat.Id, "Сегодня вы оператор Инжеста.");
                    return;
                }
                //
                await TelegramBot.SendMessageAsync(update.Message.Chat.Id, "Сообщение получено, запрос обрабатывается.");
                // Создаем заказ
                await CreateOrderAsync(update);
            }
            // Если пришло нажатие кнопки
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await ProccessOrder(update);
            }
        }

        /// <summary>
        /// Сообщение оператору с данными о заказе и кнопками взаимодействия
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<Message> SendOrderToOperatorAsync(Order order)
        {
            
            //
            var text = CreateMessageText(order);
            //
            var message = await TelegramBot.SendMessageAsync(Database.CurrentOperatorChatId, text, CreateOrderButtons());
            return message;
        }

        /// <summary>
        /// Формирование заказа
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task CreateOrderAsync(Update update)
        {
            var message = update.Message;
            var name = "Заказ от " + message.From.FirstName + " " + message.From.LastName;
            var clientName = message.From.FirstName + " " + message.From.LastName;
            var clientId = message.Chat.Id;
            var description = message.Text;
            //
            var order = new Order(name, clientName, clientId, description);
            //
            order.ClientMessageId = message.MessageId;
            //
            Database.AddOrder(order);
            // Отправляем оператору
            var messageToOperator = await SendOrderToOperatorAsync(order);
            order.OperatorMessageId = messageToOperator.MessageId;
            //
            await TelegramBot.SendMessageAsync(message.Chat.Id, "Ваш заказ добавлен в очередь");
        }

        /// <summary>
        /// Обработка заказа по нажатию кнопки в чате
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        async Task ProccessOrder(Update update)
        {
            var order = Database.Orders.FirstOrDefault(x => x.OperatorMessageId == update.CallbackQuery.Message.MessageId);
            if (order == null) return;
            if (update.CallbackQuery.Data.ToLower() == "operator.proccessing")
            {
                order.Status = OrderStatus.Обрабатывается;
                await TelegramBot.SendMessageAsync(order.ClientChatId, "Заказ в обработке.");
                var text = CreateMessageText(order);
                await TelegramBot.EditMessageAsync(order.ClientChatId, order.OperatorMessageId, text, CreateOrderButtons());
            }
            else if (update.CallbackQuery.Data.ToLower() == "operator.complete")
            {
                order.Status = OrderStatus.Выполнен;
                await TelegramBot.SendMessageAsync(order.ClientChatId, "Заказ в выполнен.");
                var text = CreateMessageText(order);
                await TelegramBot.EditMessageAsync(order.ClientChatId, order.OperatorMessageId, text, CreateOrderButtons());
            }
            //await TelegramBot.SendMessageAsync(Config.ConfigInfo.AdminChatId, "Нажата кнопка: " + update.CallbackQuery.Data.ToString());
            // Отвечаю на запрос, вызванный нажатием кнопки (иначе на кнопке висели бы часики как на неотправленном сообщении)
            await TelegramBot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
        }

        static string CreateMessageText(Order order)
        {
            string text = $"Заказ \"{order.Name}\" от {order.ClientName}.\n" +
                $"Статус заказа: {order.Status}.\n" +
                "Описанние заказа:\n" +
                $"{order.Description}";
            return text;
        }

        static InlineKeyboardButton[] CreateOrderButtons()
        {
            // Создаем кнопки
            InlineKeyboardButton proccessingButton = new InlineKeyboardButton("В обработке");
            proccessingButton.CallbackData = "Operator.Proccessing";
            InlineKeyboardButton completeButton = new InlineKeyboardButton("Выполнен");
            completeButton.CallbackData = "Operator.Complete";
            //
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                proccessingButton, completeButton
            };
            return buttons;
        }

        static async Task SendMessageFileCreated(string filename)
        {
            string text = $"Создан файл в {Config.ConfigInfo.FileWatcherCatalogPath}:\n{filename}";
            await TelegramBot.SendMessageToAdminAsync(text);
        }

    }
}
