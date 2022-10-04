using IngestManager.Entities;
using IngestManager.Models.TelegramBot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot.Types;

namespace IngestManager
{
    internal class ViewModel
    {
        /// <summary>
        /// Здесь хранятся все главные данные
        /// </summary>
        public Model.Database Model { get; } = new();

        public ViewModel()
        {
            try
            {
                TelegramBot.StartBot();
                TelegramBot.MessageRecived += CreateOrderAsync;
                TelegramBot.SendMessageWithButtonsAsync(Config.ConfigInfo.AdminChatId);
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к боту. Программа закроется");
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Формирование заказа
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task CreateOrderAsync(object? sender, EventArgs args)
        {
            var message = ((TelegramEventArgs)args).Message;
            var name = "Заказ от " + message.From.FirstName + " " + message.From.LastName;
            var clientName = message.From.FirstName + " " + message.From.LastName;
            var clientId = message.Chat.Id;
            var description = message.Text;
            //
            var order = new Order(name, clientName, clientId, description);
            Model.AddOrder(order);
            await TelegramBot.SendMessageAsync(clientId, "Ваш заказ добавлен в очередь");
        }

        public async Task CompleteOrderAsync(Order order)
        {
            order.Status = Models.OrderStatus.Выполнен;
            if (order.ClientId != null)
            {
                await TelegramBot.SendMessageAsync((long)order.ClientId, "Ваш заказ исполнен");
                //MessageBox.Show(order.ClientName + " done");
            }
        }

        /// <summary>
        /// Тестовый метод
        /// </summary>
        public void CreateEmptyOrder()
        {
            var order = new Order();
            Model.AddOrder(order);
        }

    }
}
