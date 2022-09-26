using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngestManager.Models.Telegram
{
    /// <summary>
    /// Все, что должно храниться в конфиге для Телеграм-бота
    /// </summary>
    public class ConfigInfo
    {
        // Хэш-функция Телеграм-бота
        public string? Hash { get; set; } = "abc";

    }
}
