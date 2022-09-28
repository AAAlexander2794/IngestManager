using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IngestManager.Entities;

namespace IngestManager
{
    /// <summary>
    /// 
    /// </summary>
    class Model : INotifyPropertyChanged
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

        /// <summary>
        /// Добавляет <see cref="Order"/> в <see cref="Orders"/>.
        /// </summary>
        /// <param name="order"></param>
        /// <remarks>
        /// Необходимо добавлять <see cref="Order"/> в <see cref="Orders"/> именно через этот метод,
        /// так как он сохраняет синхронность с главным потоком приложения.
        /// </remarks>
        public void AddOrder(Order order)
        {
            App.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                Orders.Add(order);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
