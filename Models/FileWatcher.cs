using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace IngestManager.Models
{
    /// <summary>
    /// Класс отслеживания файлов в указанной папке
    /// </summary>
    class FileWatcher
    {
        FileSystemWatcher Watcher { get; }

        /// <summary>
        /// Путь к директории, которую просматривает <see cref="FileSystemWatcher"/>
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        /// Путь к файлу логирования событий
        /// </summary>
        public string? LogFilePath { get; }
        
        object obj = new object();

        public delegate Task FileCreatedHandler(string filename);
        /// <summary>
        /// Событие, что файл создан
        /// </summary>
        public event FileCreatedHandler? FileCreated;

        public FileWatcher(string catalogPath, string? logFilePath = null)
        {
            DirectoryPath = catalogPath;
            LogFilePath = logFilePath;
            // Создает директорию, которую будем смотреть, если ее не было
            Directory.CreateDirectory(DirectoryPath);
            // Создаем просмотрщик
            Watcher = new FileSystemWatcher(DirectoryPath);
            //
            Watcher.Deleted += Watcher_Deleted;
            Watcher.Created += Watcher_Created;
            Watcher.Changed += Watcher_Changed;
            Watcher.Renamed += Watcher_Renamed;
            //
            Watcher.EnableRaisingEvents = true;
        }    

        // переименование файлов
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            string fileEvent = "переименован в " + e.FullPath;
            string filePath = e.OldFullPath;
            MakeLogRecord(fileEvent, filePath);
        }
        // изменение файлов
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "изменен";
            string filePath = e.FullPath;
            MakeLogRecord(fileEvent, filePath);
        }
        // создание файлов
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "создан";
            string filePath = e.FullPath;
            MakeLogRecord(fileEvent, filePath);
            string filename = e.Name ?? "";
            FileCreated?.Invoke(filename);
        }
        // удаление файлов
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "удален";
            string filePath = e.FullPath;
            MakeLogRecord(fileEvent, filePath);
        }

        private void MakeLogRecord(string fileEvent, string filePath)
        {
            // Если не указан файл для логирования, возвращаемся
            if (LogFilePath == null) return;
            // Утверждалось, что нужно для блокировки файла от стороннего открытия во время записи
            lock (obj)
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    String message = String.Format("{0} файл {1} был {2}",
                        DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), filePath, fileEvent);
                    writer.WriteLine(message);
                    writer.Flush();
                }
            }
        }
    }
}

