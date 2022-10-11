using IngestManager.Entities;
using IngestManager.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
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
    public partial class Controller
    {
        Database Database { get; }

        FileWatcher FileWatcher { get; }

        public Controller(Database database)
        {
            Database = database;
            FileWatcher = new FileWatcher(Config.ConfigInfo.FileWatcherCatalogPath);
            TelegramBot.UpdateRecieved += ProccessUpdateAsync;
            //
            Files_SetEvents();            
            //
            try
            {
                TelegramBot.StartBot();
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к боту. Программа закроется");
                Environment.Exit(0);
            }
        }



        /// <summary>
        /// Анализирует полученный от Телеграм-бота апдейт и вызывает необходимые методы обработки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        async Task ProccessUpdateAsync(object? sender, TelegramEventArgs args)
        {
            if (args == null) return;
            var update = args.Update;
            // Если пришло сообщение
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                if (update.Message.Text == null) return;
                // Начальное приветствие
                if (update.Message.Text.ToLower() == "/start")
                {
                    await TelegramBot.SendMessageAsync(update.Message.Chat.Id, "Вы подключены к боту Инжеста.");
                    return;
                }
                // Если сообщение написал админ (эта проверка раньше проверки на оператора) + другие проверки
                else if (update.Message.From?.Id == Config.ConfigInfo.AdminChatId &&
                    Regex.Replace(update.Message.Text.ToLower(), @"\s+", "") == "файл")
                {
                    var i = 0;
                    while (true)
                    {
                        var filepath = $"{Config.ConfigInfo.FileWatcherCatalogPath}\\file{i}.txt";
                        // Если файл с таким названием существует
                        if (System.IO.File.Exists(filepath))
                        {
                            i++;
                        }
                        else
                        {
                            // Просто файл создать
                            FileStream fs = new FileStream(filepath, FileMode.CreateNew);
                            //fs.FlushAsync().Wait();
                            await fs.DisposeAsync();
                            break;
                        }
                    }

                }
                // Пришло сообщение с командой стать оператором
                else if (Regex.Replace(update.Message.Text.ToLower(), @"\s+", "") == "яоператор")
                {
                    var operatorId = update.Message.Chat.Id;
                    Database.CurrentOperatorChatId = operatorId;
                    Config.ConfigInfo.OperatorChatId = operatorId;
                    Config.SaveConfig();
                    await TelegramBot.SendMessageAsync(update.Message.Chat.Id, "Сегодня вы оператор Инжеста.");
                    return;
                }
                // Если сообщение пришло от оператора
                else if (update.Message.From?.Id == Database.CurrentOperatorChatId)
                {
                    try
                    {
                        // Обрабатываем сообщение как определение принадлежности файла заказу
                        await Files_ProcessMessageAsync(update.Message.Text);
                    }
                    catch { }
                }
                // Если ничего из вышеперечисленного, то это просто клиент и он хочет сделать заказ
                else
                {
                    //
                    await TelegramBot.SendMessageAsync(update.Message.Chat.Id, "Сообщение получено, запрос обрабатывается.");
                    // Создаем заказ
                    await CreateOrderAsync(update);
                }
            }
            // Если пришло нажатие кнопки
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
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

        

        

    }
}
