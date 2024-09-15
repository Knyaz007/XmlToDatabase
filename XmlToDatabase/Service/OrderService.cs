using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace XmlToDatabase
{
    public class OrderService : IOrderService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        // Обновленный конструктор для инициализации всех зависимостей
        public OrderService(IUserRepository userRepository, IOrderRepository orderRepository, IProductRepository productRepository, IDatabaseConnectionFactory connectionFactory)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _connectionFactory = connectionFactory;
        }

        public void SaveOrders(IEnumerable<Order> orders)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var order in orders)
                        {
                            var userId = _userRepository.SaveUser(order.User, connection, transaction);
                            var orderId = _orderRepository.SaveOrder(order, userId, connection, transaction);

                            foreach (var product in order.Products)
                            {
                                _productRepository.SaveProduct(product, orderId, connection, transaction);
                            }
                        }

                        transaction.Commit(); 
                    }
                    catch (Exception)
                    {
                        transaction.Rollback(); 
                        throw;
                    }
                }
            }
        }
    }
}
