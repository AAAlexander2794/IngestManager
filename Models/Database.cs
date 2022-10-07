﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IngestManager.Entities;
using IngestManager.Models;
using Telegram.Bot.Types;

namespace IngestManager.Models
{
    /// <summary>
    /// База данных модели.
    /// </summary>
    /// <remarks>
    /// Хранит данные, с которыми работает приложение.
    /// </remarks>
    public class Database : INotifyPropertyChanged
    {
        /// <summary>
        /// Список заказов
        /// </summary>
        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        private Order? _currentOrder;
        /// <summary>
        /// Текущий выбранный заказ
        /// </summary>
        public Order? CurrentOrder
        {
            get => _currentOrder;
            set
            {
                _currentOrder = value;
                OnPropertyChanged();
            }
        }

        private long _currentOperatorChatId = Config.ConfigInfo.OperatorChatId;
        /// <summary>
        /// Текущий оператор Инжеста
        /// </summary>
        public long CurrentOperatorChatId 
        { 
            get => _currentOperatorChatId; 
            set 
            { 
                _currentOperatorChatId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Открытые заказы с короткой нумерацией для того, чтобы оператор мог выбрать,
        /// к какому заказу относится загруженны файл.
        /// </summary>
        public Dictionary<int, Order> OpenOrders { get; set; } = new Dictionary<int, Order>();

        /// <summary>
        /// Текущий файл, который был загружен в директорию
        /// </summary>
        public string? CurrentFilename { get; set; }

        /// <summary>
        /// Имена файлов, загруженные в папку, за которой смотрит <see cref="FileWatcher"/>
        /// </summary>
        public ObservableCollection<string> Filenames { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Добавляет заказ (<see cref="Order"/>) в заказы (<see cref="Orders"/>).
        /// </summary>
        /// <param name="order"></param>
        /// <remarks>
        /// Необходимо добавлять <see cref="Order"/> в <see cref="Orders"/> именно через этот метод,
        /// так как он сохраняет синхронность с главным потоком приложения.
        /// </remarks>
        public void AddOrder(Order order)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(delegate ()
            {
                Orders.Add(order);
            });
        }

        /// <summary>
        /// Обновляет список открытых заказов из <see cref="Orders"/>
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Order> RefreshCurrentOpenOrders()
        {
            OpenOrders.Clear();
            int i = 1;
            foreach (Order order in Orders.Where(x => x.Status != OrderStatus.Выполнен))
            {
                OpenOrders[i++] = order;
            }
            return OpenOrders;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
