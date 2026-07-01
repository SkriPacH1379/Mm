using System.IO;
using System.Text.Json;
using TransportTask.Models;

namespace TransportTask.Services
{
    /// <summary>
    /// Сервис для работы с JSON-файлами
    /// Отвечает за загрузку и сохранение данных
    /// </summary>
    public static class JsonService
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,           // Красивый формат с отступами
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Для совместимости с JSON
        };

        /// <summary>
        /// Загружает данные из JSON-файла
        /// </summary>
        public static TransportData Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден", path);

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<TransportData>(json, Options);
        }

        /// <summary>
        /// Сохраняет данные в JSON-файл
        /// </summary>
        public static void Save(string path, TransportData data)
        {
            string json = JsonSerializer.Serialize(data, Options);
            File.WriteAllText(path, json);
        }
    }
}