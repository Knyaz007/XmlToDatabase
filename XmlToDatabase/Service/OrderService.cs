using System.Collections.Generic;
using System.Data;

namespace XmlToDatabase
{
    public class OrderService : IOrderService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IDatabaseConnectionFactory _connectionFactory;

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
                            // Проверка и обновление/вставка пользователя
                            var existingUserId = _userRepository.GetUserId(order.User, connection, transaction);
                            if (existingUserId.HasValue)
                            {
                                // Если пользователь существует, обновляем его данные
                                _userRepository.UpdateUser(order.User, connection, transaction);
                            }
                            else
                            {
                                // Если пользователя нет, добавляем его
                                existingUserId = _userRepository.SaveUser(order.User, connection, transaction);
                            }

                            // Проверка и обновление/вставка заказа
                            var existingOrderId = _orderRepository.GetOrderId(order.Id, connection, transaction);
                            if (existingOrderId.HasValue)
                            {
                                // Если заказ существует, обновляем его
                                _orderRepository.UpdateOrder(order, existingUserId.Value, connection, transaction);
                            }
                            else
                            {
                                // Если заказа нет, добавляем его
                                existingOrderId = _orderRepository.SaveOrder(order, existingUserId.Value, connection, transaction);
                            }

                            // После сохранения заказа добавляем продукты, ссылаясь на сохраненный orderId
                            foreach (var product in order.Products)
                            {
                                if (_productRepository.ProductExists(product.Name, connection, transaction))
                                {
                                    // Если продукт существует, обновляем его
                                    _productRepository.UpdateProduct(product, connection, transaction);
                                }
                                else
                                {
                                    // Если продукта нет, добавляем его
                                    _productRepository.SaveProduct(product, existingOrderId.Value, connection, transaction);
                                }
                            }
                        }

                        // Коммит транзакции
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Откат транзакции в случае ошибки
                        transaction.Rollback();
                        Console.WriteLine($"Произошла ошибка при работе с базой данных: {ex.Message}");
                        throw;
                    }
                }
            }
        }

    }
}
