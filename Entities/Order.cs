using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        public long ClientChatId { get; }

        /// <summary>
        /// Id сообщения от клиента, в котором была отправлена информация о заказе
        /// </summary>
        public int? ClientMessageId { get; set; }

        /// <summary>
        /// Id сообщения у оператора, в котором была отправлена информация о заказе
        /// </summary>
        public int OperatorMessageId { get; set; }

        /// <summary>
        /// Дополнительное описание от клиента, если требуются пояснения по заказу
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Время создания заказа
        /// </summary>
        public DateTime Created { get; }

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

        public DateTime? Updated { get; set; }

        /// <summary>
        /// Время выполнения заказа
        /// </summary>
        public DateTime? Completed { get; set; }

        /// <summary>
        /// Коллекция имен файлов в определенной папке, которые относятся к данному заказу.
        /// </summary>
        public ObservableCollection<string> Filenames { get; set; } = new ObservableCollection<string>();

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
            ClientChatId = Config.ConfigInfo.AdminChatId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Название заказа</param>
        /// <param name="clientName">Название заказчика</param>
        /// <param name="clientChatId">Идентификатор заказчика</param>
        /// <param name="description">Описание заказа</param>
        public Order(string name, string clientName, long clientChatId, string description)
        {
            Hash = Guid.NewGuid().ToString();
            Name = name;
            ClientName = clientName;
            ClientChatId = clientChatId;
            Description = description;
            Status = OrderStatus.Получен;
            Created = DateTime.Now;
        }

        public void AddFilename(string filename)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(delegate ()
            {
                Filenames.Add(filename);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
