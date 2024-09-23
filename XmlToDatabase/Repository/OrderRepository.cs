using System;
using System.Data;

namespace XmlToDatabase
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public OrderRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Метод сохранения заказа
        public int SaveOrder(Order order, int userId, IDbConnection connection, IDbTransaction transaction)
        {
            // Проверяем, существует ли заказ с таким ID из XML
            int? existingOrderId = GetOrderIdFromXml(order.Id, connection, transaction);

            if (existingOrderId.HasValue)
            {
                // Заказ уже существует
                Console.WriteLine($"Заказ с ID {order.Id} уже существует.");
                return existingOrderId.Value;
            }
            else
            {
                // Заказ не найден, добавляем новый
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = @"
                        INSERT INTO Покупки (ID_Из_XML, ID_Пользователя, Дата_покупки, Общая_сумма)
                        VALUES (@XmlOrderId, @UserId, @Date, @Sum);
                        SELECT SCOPE_IDENTITY();"; // Получаем ID вставленной записи

                    var xmlOrderIdParam = command.CreateParameter();
                    xmlOrderIdParam.ParameterName = "@XmlOrderId";
                    xmlOrderIdParam.Value = order.Id; // ID из XML
                    command.Parameters.Add(xmlOrderIdParam);

                    var userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@UserId";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);

                    var dateParam = command.CreateParameter();
                    dateParam.ParameterName = "@Date";
                    dateParam.Value = order.Date;
                    command.Parameters.Add(dateParam);

                    var sumParam = command.CreateParameter();
                    sumParam.ParameterName = "@Sum";
                    sumParam.Value = order.Sum;
                    command.Parameters.Add(sumParam);

                    // Получаем ID нового заказа
                    int newOrderId = Convert.ToInt32(command.ExecuteScalar());
                    Console.WriteLine($"Заказ с ID {order.Id} добавлен.");
                    return newOrderId; // Возвращаем новый ID заказа
                }
            }
        }

        // Метод удаления заказа
        public void DeleteOrder(int orderId, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    DELETE FROM Покупки_Товаров WHERE ID_Покупки = @OrderId;
                    DELETE FROM Покупки WHERE ID_Покупки = @OrderId;";

                var orderIdParam = command.CreateParameter();
                orderIdParam.ParameterName = "@OrderId";
                orderIdParam.Value = orderId;
                command.Parameters.Add(orderIdParam);

                command.ExecuteNonQuery(); // Выполнение команды удаления
            }
        }

        // Метод получения существующего ID заказа
        public int? GetOrderId(int orderId, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    SELECT ID_Покупки FROM Покупки WHERE ID_Из_XML = @OrderId";

                var orderIdParam = command.CreateParameter();
                orderIdParam.ParameterName = "@OrderId";
                orderIdParam.Value = orderId;
                command.Parameters.Add(orderIdParam);

                var result = command.ExecuteScalar();
                return result != null ? (int?)Convert.ToInt32(result) : null;
            }
        }

        public int? GetOrderIdFromXml(int xmlOrderId, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "SELECT ID_Покупки FROM Покупки WHERE ID_Из_XML = @XmlOrderId";

                var xmlOrderIdParam = command.CreateParameter();
                xmlOrderIdParam.ParameterName = "@XmlOrderId";
                xmlOrderIdParam.Value = xmlOrderId;
                command.Parameters.Add(xmlOrderIdParam);

                var result = command.ExecuteScalar();
                return result != null ? (int?)Convert.ToInt32(result) : null;
            }
        }

        public void UpdateOrder(Order order, int userId, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
            UPDATE Покупки
            SET ID_Пользователя = @UserId,
                Дата_покупки = @Date,
                Общая_сумма = @Sum
            WHERE ID_Из_XML = @XmlOrderId";

                var xmlOrderIdParam = command.CreateParameter();
                xmlOrderIdParam.ParameterName = "@XmlOrderId";
                xmlOrderIdParam.Value = order.Id; // ID из XML
                command.Parameters.Add(xmlOrderIdParam);

                var userIdParam = command.CreateParameter();
                userIdParam.ParameterName = "@UserId";
                userIdParam.Value = userId;
                command.Parameters.Add(userIdParam);

                var dateParam = command.CreateParameter();
                dateParam.ParameterName = "@Date";
                dateParam.Value = order.Date;
                command.Parameters.Add(dateParam);

                var sumParam = command.CreateParameter();
                sumParam.ParameterName = "@Sum";
                sumParam.Value = order.Sum;
                command.Parameters.Add(sumParam);

                command.ExecuteNonQuery();
                Console.WriteLine($"Заказ с ID {order.Id} обновлен.");
            }
        }

    }
}