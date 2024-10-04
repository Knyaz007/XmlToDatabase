using System;
using System.Collections.Generic;
using XmlToDatabase.Utils;

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
            using (var unitOfWork = new UnitOfWork(_connectionFactory)) // Создаем новый UnitOfWork
            {
                unitOfWork.BeginTransaction(); // Начинаем транзакцию
                try
                {
                    foreach (var order in orders)
                    {
                        // Проверка и обновление/вставка пользователя
                        var existingUserId = _userRepository.GetUserId(order.User);
                        if (existingUserId.HasValue)
                        {
                            // Если пользователь существует, обновляем его данные
                            _userRepository.UpdateUser(order.User);
                        }
                        else
                        {
                            // Если пользователя нет, добавляем его
                            existingUserId = _userRepository.SaveUser(order.User);
                        }

                        // Проверка и обновление/вставка заказа
                        var existingOrderId = _orderRepository.GetOrderId(order.Id);
                        if (existingOrderId.HasValue)
                        {
                            // Если заказ существует, обновляем его
                            _orderRepository.UpdateOrder(order, existingUserId.Value);
                        }
                        else
                        {
                            // Если заказа нет, добавляем его
                            existingOrderId = _orderRepository.SaveOrder(order, existingUserId.Value);
                        }

                        // После сохранения заказа добавляем продукты
                        foreach (var product in order.Products)
                        {
                            if (_productRepository.ProductExists(product.Name))
                            {
                                // Если продукт существует, обновляем его
                                _productRepository.UpdateProduct(product);
                            }
                            else
                            {
                                // Если продукта нет, добавляем его
                                _productRepository.SaveProduct(product, existingOrderId.Value);
                            }
                        }
                    }

                    // Коммит транзакции
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    // Откат транзакции в случае ошибки
                    unitOfWork.Rollback();
                    Console.WriteLine($"Произошла ошибка при работе с базой данных: {ex.Message}");
                    throw; // Перекидываем исключение для дальнейшей обработки
                }
            }
        }
    }
}
