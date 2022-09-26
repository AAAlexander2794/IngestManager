using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IngestManager.Models;

namespace IngestManager.Entities
{
    /// <summary>
    /// Заказ, здесь вся информация о том, кто и что хочет получить от Инжеста
    /// </summary>
    internal class Order : INotifyPropertyChanged
    {
        /// <summary>
        /// Некоторый идентификатор (уникальный или нет?)
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Уникальный большой идентификатор заказа
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// Название заказа
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Клиент, то есть тот, кто делает заказ
        /// </summary>
        public string Client { get; }

        /// <summary>
        /// Дополнительное описание от клиента, если требуются пояснения по заказу
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Время создания заказа
        /// </summary>
        public DateTime Created { get; }

        public DateTime? Updated { get; set; }

        /// <summary>
        /// Время выполнения заказа
        /// </summary>
        public DateTime? Completed { get; set; }

        public string? FileName { get; set; }

        public string? FilePath { get; set; }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Order()
        {
            Hash = Guid.NewGuid().ToString();
            Name = "";
            Client = "Unknown";
            Description = "";
            Status = OrderStatus.Получен;
            Created = DateTime.Now;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
