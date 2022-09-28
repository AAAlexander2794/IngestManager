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
    public class Order : INotifyPropertyChanged
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
        /// Имя клиента, то есть тот, кто делает заказ
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// Идентификатор клиента (запланирован как Id чата в Телеграме)
        /// </summary>
        public long? ClientId { get; }

        /// <summary>
        /// Дополнительное описание от клиента, если требуются пояснения по заказу
        /// </summary>
        public string Description { get; }

        private OrderStatus _status;
        /// <summary>
        /// Статус заказа
        /// </summary>
        public OrderStatus Status 
        { 
            get => _status; 
            set 
            {
                _status = value;
                OnPropertyChanged();
            }
        }

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
        /// Конструктор по умолчанию (по хорошему следует удалить)
        /// </summary>
        public Order()
        {
            Hash = Guid.NewGuid().ToString();
            Status = OrderStatus.Получен;
            Created = DateTime.Now;
            //
            Name = "Empty name " + Created;
            ClientName = "Unknown";
            Description = "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Название заказа</param>
        /// <param name="clientName">Название заказчика</param>
        /// <param name="clientId">Идентификатор заказчика</param>
        /// <param name="description">Описание заказа</param>
        public Order(string name, string clientName, long clientId, string description)
        {
            Hash = Guid.NewGuid().ToString();
            Name = name;
            ClientName = clientName;
            ClientId = clientId;
            Description = description;
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
