using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace IngestManager.Entities
{
    /// <summary>
    /// Класс хранения аргументов при возникновении событий в клиенте Телеграма.
    /// </summary>
    /// 
    /// Конечно лучше не опираться на класс из подключаемой сборки, но пока некритично.
    /// 
    internal class TelegramEventArgs : EventArgs
    {
        public Message Message { get; set; }

        public TelegramEventArgs(Message message)
        {
            Message = message;
        }
    }
}
