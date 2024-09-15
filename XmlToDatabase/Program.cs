using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XmlToDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            string xmlFilePath = "orders.xml";

            // Проверяем, существует ли файл
            if (!File.Exists(xmlFilePath))
            {
                Console.WriteLine($"Файл {xmlFilePath} не найден.");
                return;
            }

            // Строка подключения к базе данных
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\y-men\Downloads\XmlToDatabase\XmlToDatabase\purchase_sale.mdf; Integrated Security=True;";

            try
            {
                // Инициализация фабрики подключения и репозиториев
                IDatabaseConnectionFactory connectionFactory = new DatabaseConnectionFactory(connectionString);
                IUserRepository userRepository = new UserRepository(connectionFactory);
                IOrderRepository orderRepository = new OrderRepository(connectionFactory);
                IProductRepository productRepository = new ProductRepository(connectionFactory);

                // Инициализация сервиса для работы с заказами
                IOrderService orderService = new OrderService(userRepository, orderRepository, productRepository, connectionFactory);

                // Загрузка данных из XML
                var orders = OrderLoader.LoadOrdersFromXml(xmlFilePath);

                if (!orders.Any())
                {
                    Console.WriteLine("Нет заказов для загрузки.");
                    return;
                }

                // Сохранение данных в базе данных
                orderService.SaveOrders(orders);

                Console.WriteLine("Данные успешно загружены в базу данных.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при работе с базой данных: {ex.Message}");
            }
        }
    }
}
