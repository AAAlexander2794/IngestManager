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
        FileSystemWatcher watcher;
        string _logFilePath;
        object obj = new object();
        bool enabled = true;

        public FileWatcher(string catalogPath, string logFilePath)
        {
            Directory.CreateDirectory(catalogPath);
            watcher = new FileSystemWatcher(catalogPath);
            _logFilePath = logFilePath;
            watcher.Deleted += Watcher_Deleted;
            watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;
            //
            watcher.EnableRaisingEvents = true;
        }

        public FileWatcher(string catalogPath)
        {
            Directory.CreateDirectory(catalogPath);
            watcher = new FileSystemWatcher(catalogPath);
            _logFilePath = Config.ConfigInfo.FileWatcherLogFilePath;
            watcher.Deleted += Watcher_Deleted;
            watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;
            //
            watcher.EnableRaisingEvents = true;
        }        

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            enabled = false;
        }
        // переименование файлов
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            string fileEvent = "переименован в " + e.FullPath;
            string filePath = e.OldFullPath;
            RecordEntry(fileEvent, filePath);
        }
        // изменение файлов
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "изменен";
            string filePath = e.FullPath;
            RecordEntry(fileEvent, filePath);
        }
        // создание файлов
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "создан";
            string filePath = e.FullPath;
            RecordEntry(fileEvent, filePath);
        }
        // удаление файлов
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string fileEvent = "удален";
            string filePath = e.FullPath;
            RecordEntry(fileEvent, filePath);
        }

        private void RecordEntry(string fileEvent, string filePath)
        {
            lock (obj)
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
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

