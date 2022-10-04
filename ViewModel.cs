using IngestManager.Entities;
using IngestManager.Models;
using IngestManager.Models;
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
        public Database Database { get; }

        public Controller Controller { get; }

        public ViewModel()
        {
            // Создаем основные сущности
            Database = new Database();
            Controller = new Controller(Database);
            //
            try
            {
                TelegramBot.StartBot();
                //Controller.SendOrderToOperatorAsync(new Order());
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к боту. Программа закроется");
                Environment.Exit(0);
            }
        }

        

        //public async Task CompleteOrderAsync(Order order)
        //{
        //    order.Status = Models.OrderStatus.Выполнен;
        //    if (order.ClientChatId != null)
        //    {
        //        await TelegramBot.SendMessageAsync((long)order.ClientChatId, "Ваш заказ исполнен");
        //        //MessageBox.Show(order.ClientName + " done");
        //    }
        //}

        /// <summary>
        /// Тестовый метод
        /// </summary>
        public void CreateEmptyOrder()
        {
            var order = new Order();
            Database.AddOrder(order);
        }

    }
}
