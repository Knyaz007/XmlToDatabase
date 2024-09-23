using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

namespace XmlToDatabase
{
    public class OrderLoader
    {
        private static string processedFilesPath = "processedFiles.json";
        private static string processedOrdersPath = "processedOrders.json";
        private static Dictionary<string, string> processedFiles = LoadProcessedFiles();
        private static Dictionary<int, string> processedOrders = LoadProcessedOrders();

        public static IEnumerable<Order> LoadOrdersForProcessing(string filePath)
        {
            // Проверяем, был ли файл уже обработан и изменялся ли он
            if (IsFileProcessed(filePath, out bool fileChanged))
            {
                if (!fileChanged)
                {
                    Console.WriteLine("Файл уже обработан и не изменялся.");
                    return Enumerable.Empty<Order>();
                }
                else
                {
                    // Если файл был обработан, но изменён, вычисляем изменённые заказы
                    Console.WriteLine("Файл изменён. Проверяем изменённые заказы.");
                    return GetChangedOrders(filePath);
                }
            }
            else
            {
                // Если файл не был обработан, загружаем все заказы
                Console.WriteLine("Файл не обработан. Загружаем все заказы.");
                var orders = LoadAllOrders(filePath);

                // Сохраняем хэш файла
                SaveProcessedFile(filePath);

                // Сохраняем все заказы
                foreach (var order in orders)
                {
                    SaveProcessedOrder(order);
                }

                return orders; // Возвращаем все заказы для загрузки
            }
        }

        private static IEnumerable<Order> GetChangedOrders(string filePath)
        {
            var orders = LoadAllOrders(filePath);
            var changedOrders = new List<Order>();

            foreach (var order in orders)
            {
                if (IsOrderNewOrChanged(order))
                {
                    // Если заказ новый или изменён, добавляем его в список
                    changedOrders.Add(order);
                    SaveProcessedOrder(order); // Сохраняем изменённый заказ
                }
            }

            // Обновляем хэш файла
            SaveProcessedFile(filePath);

            return changedOrders;
        }

        public static IEnumerable<Order> LoadAllOrders(string filePath)
        {
            var document = XDocument.Load(filePath);
            return from orderElement in document.Descendants("order")
                   select new Order
                   {
                       Id = (int)orderElement.Element("no"),
                       Date = DateTime.ParseExact((string)orderElement.Element("reg_date"), "yyyy.MM.dd", null),
                       Sum = (decimal)orderElement.Element("sum"),
                       User = new User
                       {
                           FullName = (string)orderElement.Element("user").Element("fio"),
                           Email = (string)orderElement.Element("user").Element("email")
                       },
                       Products = orderElement.Elements("product").Select(product => new Product
                       {
                           Name = (string)product.Element("name"),
                           Quantity = (int)product.Element("quantity"),
                           Price = (decimal)product.Element("price")
                       }).ToList()
                   };
        }

        private static bool IsOrderNewOrChanged(Order order)
        {
            string currentOrderHash = GetOrderHash(order);

            // Проверяем, есть ли запись о заказе и совпадает ли хэш
            if (processedOrders.TryGetValue(order.Id, out string previousHash))
            {
                return previousHash != currentOrderHash; // Если хэш не совпадает, заказ изменён
            }

            return true; // Заказ новый
        }

        private static void SaveProcessedOrder(Order order)
        {
            string orderHash = GetOrderHash(order);
            processedOrders[order.Id] = orderHash; // Обновляем хэш заказа
            SaveProcessedOrders(); // Сохраняем словарь в файл
        }

        private static string GetOrderHash(Order order)
        {
            using (var md5 = MD5.Create())
            {
                // Хэшируем отдельные поля заказа
                string dateHash = GetFieldHash(order.Date.ToString("yyyy-MM-ddTHH:mm:ss"));
                string sumHash = GetFieldHash(order.Sum.ToString());
                string userHash = GetFieldHash($"{order.User.FullName}{order.User.Email}");

                // Хэшируем каждый товар
                string productHashes = string.Join("", order.Products.Select(p => GetFieldHash($"{p.Name}{p.Quantity}{p.Price}")));

                // Комбинируем хэши всех полей
                string orderData = $"{dateHash}{sumHash}{userHash}{productHashes}";

                // Генерация окончательного хэша заказа
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(orderData));
                string orderData2 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        // Вспомогательный метод для хэширования полей
        private static string GetFieldHash(string fieldData)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(fieldData));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static bool IsFileProcessed(string filePath, out bool fileChanged)
        {
            string currentFileHash = GetFileHash(filePath);

            if (processedFiles.TryGetValue(filePath, out string previousHash))
            {
                fileChanged = previousHash != currentFileHash; // Если хэш изменился, файл изменён
                return true; // Файл обработан ранее
            }

            fileChanged = false;
            return false; // Файл не обработан
        }

        private static void SaveProcessedFile(string filePath)
        {
            string fileHash = GetFileHash(filePath);
            processedFiles[filePath] = fileHash; // Обновляем хэш файла
            SaveProcessedFiles(); // Сохраняем словарь
        }

        private static string GetFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private static Dictionary<string, string> LoadProcessedFiles()
        {
            if (File.Exists(processedFilesPath))
            {
                var json = File.ReadAllText(processedFilesPath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            return new Dictionary<string, string>();
        }

        private static void SaveProcessedFiles()
        {
            try
            {
                var json = JsonSerializer.Serialize(processedFiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(processedFilesPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении файлов: {ex.Message}");
            }
        }

        private static Dictionary<int, string> LoadProcessedOrders()
        {
            if (File.Exists(processedOrdersPath))
            {
                var json = File.ReadAllText(processedOrdersPath);
                return JsonSerializer.Deserialize<Dictionary<int, string>>(json) ?? new Dictionary<int, string>();
            }
            return new Dictionary<int, string>();
        }

        private static void SaveProcessedOrders()
        {
            try
            {
                var json = JsonSerializer.Serialize(processedOrders, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(processedOrdersPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении заказов: {ex.Message}");
            }
        }
    }
}
