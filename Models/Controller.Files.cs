using IngestManager.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Telegram.Bot.Types;

namespace IngestManager.Models
{
    /// <summary>
    /// Часть класса по работе с файлами
    /// </summary>
    public partial class Controller
    {
        /// <summary>
        /// Очередь имен файлов, загруженные в папку, за которой смотрит <see cref="FileWatcher"/>
        /// </summary>
        private ObservableCollection<string> Files_FilenamesQueue { get; } = new ObservableCollection<string>();

        // <summary>
        /// Текущий файл, который был загружен в директорию
        /// </summary>
        private string? Files_CurrentFilename { get; set; }

        /// <summary>
        /// Открытые заказы с короткой нумерацией для того, чтобы оператор мог выбрать,
        /// к какому заказу относится загруженны файл.
        /// </summary>
        public Dictionary<int, Order> Files_OpenOrders { get; set; } = new Dictionary<int, Order>();

        private async Task Files_ProcessMessage(string messageText)
        {
            try
            {
                var key = int.Parse(messageText);
                var order = Files_AddFilenameToOrder(key);
                // Сообщение заказчику
                await TelegramBot.SendMessageAsync(order.ClientChatId, $"По вашему заказу загружен файл:\n{order.FilePath}.");
                // Сообщение оператору
                await TelegramBot.SendMessageAsync(Config.ConfigInfo.OperatorChatId, $"Файл добавлен к заказу, заказчик оповещен.");
            }
            catch { }
        }

        /// <summary>
        /// При получении номера заказа от оператора
        /// </summary>
        /// <param name="orderKey"></param>
        private Order? Files_AddFilenameToOrder(int orderKey)
        {
            // Если текущее имя файла не выбрано, возвращаемся
            if (Files_CurrentFilename == null) return null;
            // Ищем заказ по полученному ключу
            var order = Files_OpenOrders[orderKey];
            // Добавляем к заказу путь файла
            order.FilePath = Files_CurrentFilename;
            // Удаляем имя файла из очереди
            Files_FilenamesQueue.Remove(Files_CurrentFilename);
            // Очищаем 
            Files_OpenOrders.Clear();
            Files_CurrentFilename = null;
            return order;
        }


        private void Files_SetEvents()
        {
            Files_FilenamesQueue.CollectionChanged += Files_FilenamesQueue_CollectionChanged;
        }

        /// <summary>
        /// Добавление в очередь на обработку нового файла 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private Task Files_AddFilenameToQueue(string filename)
        {
            Files_FilenamesQueue.Add(filename);
            return Task.CompletedTask;
        }

        async Task SendMessageFileCreated(string filename)
        {
            Database.CurrentFilename = filename;
            var openOrders = Database.RefreshCurrentOpenOrders();
            if (openOrders.Count == 0)
            {
                await TelegramBot.SendMessageAsync(Config.ConfigInfo.OperatorChatId,
                    $"Создан файл {Config.ConfigInfo.FileWatcherCatalogPath}{filename}, " +
                    "однако нет активных заказов.");
                return;
            }
            string text = $"Создан файл {Config.ConfigInfo.FileWatcherCatalogPath}{filename}.\n" +
                "Выберите, к какому из заказов относится файл:\n\n";
            for (int i = 0; i < openOrders.Count; i++)
            {
                /// Ключи в <see cref="Database.OpenOrders"/> начинаются с 1
                text = text + $"{i + 1}) {CreateMessageText(openOrders[i + 1])}\n";
            }
            await TelegramBot.SendMessageAsync(Config.ConfigInfo.OperatorChatId, text);
        }

        private async void Files_FilenamesQueue_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Database.FilenamesQueue.Count == 0) return;
            var filename = Database.FilenamesQueue.First();
            await SendMessageFileCreated(filename);
        }

        /// <summary>
        /// Обновляет список открытых заказов из <see cref="Orders"/>
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, Order> Files_RefreshOpenOrders(List<Order> orders)
        {
            Files_OpenOrders.Clear();
            int i = 1;
            foreach (Order order in orders.Where(x => x.Status != OrderStatus.Выполнен))
            {
                Files_OpenOrders[i++] = order;
            }
            return Files_OpenOrders;
        }
    }
}
