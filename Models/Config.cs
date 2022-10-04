using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IngestManager.Entities;

namespace IngestManager.Models;

/// <summary>
/// Читает файл конфигурации, хранит в себе настройки
/// </summary>
internal static class Config
{
    /// <summary>
    /// Хранит настройки, непосредственно считанные с файла
    /// </summary>
    public static ConfigInfo ConfigInfo { get; }

    /// <summary>
    /// Название файла конфигурации
    /// </summary>
    static string FileName { get; } = "TelegramConfig.xml";

    /// <summary>
    /// Читает файл конфигурации, если возникает ошибка, пересоздает новый, сообщает, закрывает программу.
    /// </summary>
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
            // Создает пустой файл конфигурации (с данными-заглушкой)
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
