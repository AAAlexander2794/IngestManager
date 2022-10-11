using IngestManager.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telegram.Bot.Types;

namespace IngestManager.Models
{
    /// <summary>
    /// Часть класса по работе с файлами
    /// </summary>
    public partial class Controller
    {
        

        // <summary>
        /// Текущий файл, который был загружен в директорию
        /// </summary>
        private string? Files_CurrentFilename { get; set; }

        /// <summary>
        /// Открытые заказы с короткой нумерацией для того, чтобы оператор мог выбрать,
        /// к какому заказу относится загруженны файл.
        /// </summary>
        private Dictionary<int, Order> Files_OpenOrders { get; set; } = new Dictionary<int, Order>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageText"></param>
        /// <returns></returns>
        private async Task Files_ProcessMessageAsync(string messageText)
        {
            if (Files_CurrentFilename == null)
            {
                await TelegramBot.SendMessageToAdminAsync($"Возникла ошибка при обработке сообщения. " +
                    $"{Files_CurrentFilename} отстутсвует. Сообщение:\n{messageText}");
                return;
            }
            try
            {
                var key = int.Parse(messageText);
                var order = Files_AddFilenameToOrder(key);
                // Сообщение заказчику
                await TelegramBot.SendMessageAsync(order.ClientChatId, $"По вашему заказу загружен файл:\n{Files_CurrentFilename}.");
                // Сообщение оператору
                await TelegramBot.SendMessageAsync(Config.ConfigInfo.OperatorChatId, $"Файл добавлен к заказу, заказчик оповещен.");
                // Очищаем 
                Files_OpenOrders.Clear();
                Files_CurrentFilename = null;
                // Запускаем следующий в очереди (наверное, можно и не await)
                Files_SendMessageNewFileAsync();
            }
            catch { throw; }
        }

        /// <summary>
        /// При получении номера заказа от оператора
        /// </summary>
        /// <param name="orderKey">Ключ из словаря с заказами</param>
        private Order? Files_AddFilenameToOrder(int orderKey)
        {
            // Если текущее имя файла не выбрано, возвращаемся
            if (Files_CurrentFilename == null) return null;
            // Ищем заказ по полученному ключу
            var order = Files_OpenOrders[orderKey];
            // Добавляем к заказу путь файла
            order.AddFilename(Files_CurrentFilename);
            // Удаляем имя файла из очереди
            Database.RemoveFilenameFromQueue(Files_CurrentFilename);
            //
            return order;
        }

        /// <summary>
        /// Задает привязку к событиям методов, относящихся к обработке файлов
        /// </summary>
        private void Files_SetEvents()
        {
            // Если файл создан в папке, добавляем в очередь
            FileWatcher.FileCreated += Files_AddFilenameToQueue;
            // Если в очереди что-то поменялось, запускаем метод
            Database.FilenamesQueue.CollectionChanged += Files_FilenamesQueue_CollectionChanged;
        }

        /// <summary>
        /// Добавление в очередь на обработку нового файла
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <remarks>Метод нужен для обработки события от <see cref="FileWatcher"/></remarks>
        private Task Files_AddFilenameToQueue(string filename)
        {
            Database.AddFilenameToQueue(filename);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <remarks>
        /// Запуск происходит в случаях: когда в очередь файлов добавляется новый или когда от оператора
        /// приходит сообщение касательно обработки текущего файла.
        /// </remarks>
        async Task Files_SendMessageNewFileAsync()
        {
            // Если в очереди файлов нет, то возвращаемся
            if (Database.FilenamesQueue.Count == 0) return;
            // Если сейчас уже есть текущий файл, значит оператор еще не ответил на предыдущий запрос
            if (Files_CurrentFilename != null) return;
            // 
            Files_CurrentFilename = Database.FilenamesQueue.First();
            // Получаем только открытые заказы
            var openOrders = Files_RefreshOpenOrders(Database.Orders.ToList());
            // Если открытых заказов нет, возвращаемся
            if (openOrders.Count == 0)
            {
                await TelegramBot.SendMessageAsync(Config.ConfigInfo.OperatorChatId,
                    $"Создан файл {Config.ConfigInfo.FileWatcherCatalogPath}{Files_CurrentFilename}, " +
                    "однако нет активных заказов.");
                return;
            }
            // Формируем текст сообщения оператору
            string text = $"Создан файл {Config.ConfigInfo.FileWatcherCatalogPath}{Files_CurrentFilename}.\n" +
                "Выберите, к какому из заказов относится файл:\n\n";
            // Открытые заказы в текст сообщения
            for (int i = 0; i < openOrders.Count; i++)
            {
                /// Ключи в <see cref="Files_OpenOrders"/> начинаются с 1
                text = text + $"{i + 1}) {CreateMessageText(openOrders[i + 1])}\n";
            }
            // Отправляем оператору
            await TelegramBot.SendMessageAsync(Config.ConfigInfo.OperatorChatId, text);
            // debug
            //await TelegramBot.SendMessageToAdminAsync($"message sent. {Files_CurrentFilename}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Просто прокладка.</remarks>
        private void Files_FilenamesQueue_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Database.FilenamesQueue.Count == 0) return;
            Files_SendMessageNewFileAsync();
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
