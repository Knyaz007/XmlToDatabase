using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace XmlToDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            string xmlFilePath = "orders.xml";
             
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\y-men\Downloads\XmlToDatabase\XmlToDatabase\purchase_sale.mdf; Integrated Security=True;";
           
            IDatabaseConnectionFactory connectionFactory = new DatabaseConnectionFactory(connectionString);
            IUserRepository userRepository = new UserRepository(connectionFactory);
            IOrderRepository orderRepository = new OrderRepository(connectionFactory);
            IProductRepository productRepository = new ProductRepository(connectionFactory);

            IOrderService orderService = new OrderService(userRepository, orderRepository, productRepository, connectionFactory);

            var orders = OrderLoader.LoadOrdersFromXml(xmlFilePath);         
            orderService.SaveOrders(orders);

            Console.WriteLine("Данные успешно загружены в базу данных.");
        }
    }


}
