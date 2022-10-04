﻿using IngestManager.Entities;
using IngestManager.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace IngestManager.Models
{
    public class Controller
    {
        Database Database { get; }

        public Controller(Database database)
        {
            Database = database;
            TelegramBot.OnUpdate += ProccessUpdate;
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
                // Начальное приветствие
                if (update.Message.Text != null && update.Message.Text.ToLower() == "/start")
                {
                    await TelegramBot.SendMessageAsync(update.Message.Chat.Id, "Вы подключены к боту Инжеста.");
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
            //
            var text = $"Заказ \"{order.Name}\" от {order.ClientName}.\n" +
                "Описанние заказа:\n" + 
                $"{order.Description}";
            //
            var message = await TelegramBot.SendMessageAsync(Database.CurrentOperatorChatId, text, buttons);
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
            }
            else if (update.CallbackQuery.Data.ToLower() == "operator.complete")
            {
                order.Status = OrderStatus.Выполнен;
                await TelegramBot.SendMessageAsync(order.ClientChatId, "Заказ в выполнен.");
            }
            //await TelegramBot.SendMessageAsync(Config.ConfigInfo.AdminChatId, "Нажата кнопка: " + update.CallbackQuery.Data.ToString());
            // Отвечаю на запрос, вызванный нажатием кнопки (иначе на кнопке висели бы часики как на неотправленном сообщении)
            await TelegramBot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
        }


    }
}