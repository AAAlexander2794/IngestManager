using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        /// Очередь имен файлов, загруженные в папку, за которой смотрит <see cref="FileWatcher"/>
        /// </summary>
        /// <remarks>
        /// Когда в папку загружается файл, бот должен уточнить у оператора, к какому заказу файл относится,
        /// оператор отвечает не мгновенно, к этому моменту может загрузиться в папку еще один файл, возникает очередь.
        /// </remarks>
        public ObservableCollection<string> FilenamesQueue { get; } = new ObservableCollection<string>();


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

        public void AddFilenameToQueue(string filename)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(delegate ()
            {
                FilenamesQueue.Add(filename);
            });
        }

        public void RemoveFilenameFromQueue(string filename)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(delegate ()
            {
                FilenamesQueue.Remove(filename);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
