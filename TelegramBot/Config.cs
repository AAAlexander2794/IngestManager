using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IngestManager.Models.TelegramBot
{
    /// <summary>
    /// Читает файл конфигурации для Телеграма, хранит в себе настройки
    /// </summary>
    internal static class Config
    {
        /// <summary>
        /// Хранит настройки, непосредственно считанные с файла
        /// </summary>
        public static ConfigInfo ConfigInfo { get; }

        static string FileName { get; } = "TelegramConfig.xml";

        static Config()
        {
            try
            {
                // Чтение файла
                System.Xml.Serialization.XmlSerializer reader =
                    new System.Xml.Serialization.XmlSerializer(typeof(ConfigInfo));
                StreamReader file = new StreamReader(
                    Path.GetFullPath(FileName));
                // 
                if (reader.Deserialize(file) is not ConfigInfo configInfo) 
                { 
                    throw new Exception("Файл " + FileName + " отсутствует или поврежден."); 
                }
                //
                ConfigInfo = configInfo;
                file.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //
                ConfigInfo = new ConfigInfo();
                WriteConfig(ConfigInfo, FileName);
                MessageBox.Show("Файл " + FileName + " создан. Заполните его данными и перезапустите программу.");
                //
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Записывает в файл конфиг текущую конфигурацию
        /// </summary>
        static void WriteConfig(ConfigInfo configInfo, string fileName)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(ConfigInfo));

            var path = Path.GetFullPath(fileName);
            FileStream file = File.Create(path);

            writer.Serialize(file, configInfo);
            file.Close();
        }
    }
}
