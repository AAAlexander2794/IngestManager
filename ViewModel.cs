using IngestManager.Entities;
using IngestManager.Models.TelegramBot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace IngestManager
{
    internal class ViewModel
    {
        /// <summary>
        /// Здесь хранится все главные данные
        /// </summary>
        public Model Model { get; } = new();

        public ViewModel()
        {
            TelegramBot.StartBot();
            TelegramBot.MessageRecived += Message;
        }

        public void AddOrder(Order order)
        {
            Model.Orders.Add(order);
        }

        /// <summary>
        /// Формирование заказа
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static Order CreateOrder(Message message)
        {
            var name = "Заказ от " + message.SenderChat.FirstName + " " + message.SenderChat.LastName;
            var clientName = message.SenderChat.FirstName + " " + message.SenderChat.LastName;
            var clientId = message.Chat.Id;
            var description = message.Text;
            //
            var order = new Order(name, clientName, clientId, description);

            return order;
        }

        /// <summary>
        /// Тестовый метод
        /// </summary>
        public void CreateEmptyOrder()
        {
            var order = new Order();
            AddOrder(order);
        }

        public async Task Message()
        {
            await TelegramBot.SendMessage();
        }

        public async Task Message(object sender, EventArgs args)
        {
            await TelegramBot.SendMessage();
        }
    }
}
