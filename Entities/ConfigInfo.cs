using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngestManager.Entities
{
    /// <summary>
    /// Все, что должно храниться в файле конфигурации
    /// </summary>
    public class ConfigInfo
    {
        /// <summary>
        /// Хэш-функция Телеграм-бота
        /// </summary>
        public string Hash { get; set; } = "abc";

        /// <summary>
        /// Id чата с админом, куда возможно стоит присылать сообщения
        /// </summary>
        public long AdminChatId { get; set; } = 12345;

        /// <summary>
        /// Id чата с оператором, который обрабатывает заказы клиентов
        /// </summary>
        public long OperatorChatId { get; set; } = 12345;

        /// <summary>
        /// Каталог, в котором <see cref="Models.FileWatcher"/> будет мониторить файлы на предмет изменений
        /// </summary>
        public string FileWatcherCatalogPath { get; set; } = "C:\\Temp";

        /// <summary>
        /// Путь к файлу логов <see cref="Models.FileWatcher"/>
        /// </summary>
        public string FileWatcherLogFilePath { get; set; } = "C:\\file_log.txt";

    }
}
