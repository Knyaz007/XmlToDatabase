using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace XmlToDatabase
{
    public class OrderLoader
    {
        private static string processedFilesPath = "processedFiles.json";
        private static Dictionary<string, string> processedFiles = LoadProcessedFiles();

        public static IEnumerable<Order> LoadOrdersFromXml(string filePath)
        {
            if (IsFileProcessed(filePath))
            {
                Console.WriteLine("Файл уже обработан или не изменялся.");
                return Enumerable.Empty<Order>();
            }

            var document = XDocument.Load(filePath);
            var orders = from orderElement in document.Descendants("order")
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

            
            SaveProcessedFile(filePath);

            return orders;
        }

        private static bool IsFileProcessed(string filePath)
        {
            string currentFileHash = GetFileHash(filePath);

             
            if (processedFiles.TryGetValue(filePath, out string previousHash))
            {
                return previousHash == currentFileHash;  
            }

            return false;
        }

        private static void SaveProcessedFile(string filePath)
        {
            string fileHash = GetFileHash(filePath);
            processedFiles[filePath] = fileHash;  
            SaveProcessedFiles();  
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
            var json = JsonSerializer.Serialize(processedFiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(processedFilesPath, json);
        }
    }

}
