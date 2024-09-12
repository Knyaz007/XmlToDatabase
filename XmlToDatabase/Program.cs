using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;

namespace XmlToDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            string xmlFilePath = "orders.xml";
            string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=purchase_sale;Trusted_Connection=True;";
            var orders = LoadOrdersFromXml(xmlFilePath);
            SaveOrdersToDatabase(orders, connectionString);
            Console.WriteLine("Данные успешно загружены в базу данных.");
        }

        static IEnumerable<Order> LoadOrdersFromXml(string filePath)
        {
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

            return orders;
        }

        static void SaveOrdersToDatabase(IEnumerable<Order> orders, string connectionString)
        {
            using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var order in orders)
                {
                    int userId;
                    using (var userCommand = connection.CreateCommand())
                    {
                        userCommand.CommandText = @"
                            IF NOT EXISTS (SELECT 1 FROM Пользователи WHERE Электронная_почта = @Электронная_почта)
                            BEGIN
                                INSERT INTO Пользователи (Имя_пользователя, Электронная_почта, Имя, Фамилия)
                                VALUES (@Имя_пользователя, @Электронная_почта, @Имя, @Фамилия);
                            END";
                        userCommand.Parameters.AddWithValue("@Имя_пользователя", order.User.Email);
                        userCommand.Parameters.AddWithValue("@Электронная_почта", order.User.Email);
                        userCommand.Parameters.AddWithValue("@Имя", order.User.FullName.Split(' ')[1]);
                        userCommand.Parameters.AddWithValue("@Фамилия", order.User.FullName.Split(' ')[0]);
                        userCommand.ExecuteNonQuery();

                        userCommand.CommandText = "SELECT ID_Пользователя FROM Пользователи WHERE Электронная_почта = @Электронная_почта";
                        userId = (int)userCommand.ExecuteScalar();
                    }

                    int purchaseId;
                    using (var purchaseCommand = connection.CreateCommand())
                    {
                        purchaseCommand.CommandText = @"
                            INSERT INTO Покупки (ID_Пользователя, Дата_покупки, Общая_сумма)
                            VALUES (@ID_Пользователя, @Дата_покупки, @Общая_сумма);
                            SELECT SCOPE_IDENTITY();";
                        purchaseCommand.Parameters.AddWithValue("@ID_Пользователя", userId);
                        purchaseCommand.Parameters.AddWithValue("@Дата_покупки", order.Date.ToString("yyyy-MM-dd"));
                        purchaseCommand.Parameters.AddWithValue("@Общая_сумма", order.Sum);
                        purchaseId = Convert.ToInt32(purchaseCommand.ExecuteScalar());
                    }

                    using (var productCommand = connection.CreateCommand())
                    {
                        foreach (var product in order.Products)
                        {
                            productCommand.Parameters.Clear();

                            productCommand.CommandText = @"
                                IF NOT EXISTS (SELECT 1 FROM Товары WHERE Название = @Название)
                                BEGIN
                                    INSERT INTO Товары (Название, Цена, Количество_на_складе)
                                    VALUES (@Название, @Цена, 0);
                                END";
                            productCommand.Parameters.AddWithValue("@Название", product.Name);
                            productCommand.Parameters.AddWithValue("@Цена", product.Price);
                            productCommand.ExecuteNonQuery();

                            productCommand.Parameters.Clear();

                            productCommand.CommandText = "SELECT ID_Товара FROM Товары WHERE Название = @Название";
                            productCommand.Parameters.AddWithValue("@Название", product.Name);
                            int productId = (int)productCommand.ExecuteScalar();

                            productCommand.Parameters.Clear();

                            productCommand.CommandText = @"
                                INSERT INTO Покупки_Товаров (ID_Покупки, ID_Товара, Количество, Общая_стоимость)
                                VALUES (@ID_Покупки, @ID_Товара, @Количество, @Общая_стоимость)";
                            productCommand.Parameters.AddWithValue("@ID_Покупки", purchaseId);
                            productCommand.Parameters.AddWithValue("@ID_Товара", productId);
                            productCommand.Parameters.AddWithValue("@Количество", product.Quantity);
                            productCommand.Parameters.AddWithValue("@Общая_стоимость", product.Price * product.Quantity);
                            productCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Sum { get; set; }
        public User User { get; set; }
        public List<Product> Products { get; set; }
    }

    public class User
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
