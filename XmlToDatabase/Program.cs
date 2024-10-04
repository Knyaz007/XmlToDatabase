using System;
using System.Linq;
using XmlToDatabase.Utils;

namespace XmlToDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            // Строка подключения к базе данных
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\y-men\Downloads\XmlToDatabase\XmlToDatabase\purchase_sale.mdf; Integrated Security=True;";
            IDatabaseConnectionFactory connectionFactory = new DatabaseConnectionFactory(connectionString);
            IUnitOfWork unitOfWork = new UnitOfWork(connectionFactory);

            try
            {
                unitOfWork.BeginTransaction();

                IUserRepository userRepository = new UserRepository(unitOfWork);
                IOrderRepository orderRepository = new OrderRepository(unitOfWork);
                IProductRepository productRepository = new ProductRepository(unitOfWork);
                IOrderService orderService = new OrderService(userRepository, orderRepository, productRepository, connectionFactory);

                var orders = OrderLoader.LoadOrdersForProcessing("orders.xml");

                if (orders.Any())
                {
                    orderService.SaveOrders(orders);
                }

                unitOfWork.Commit();
                Console.WriteLine("Данные успешно сохранены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                unitOfWork.Rollback();
            }
            finally
            {
                // Закрытие соединения
                unitOfWork.Dispose();
            }
        }
    }
}
