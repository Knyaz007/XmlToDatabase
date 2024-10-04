using System;
using System.Data;
using XmlToDatabase.Utils;

namespace XmlToDatabase
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Метод сохранения заказа
        public int SaveOrder(Order order, int userId)
        {
            // Проверяем, существует ли заказ с таким ID из XML
            int? existingOrderId = GetOrderIdFromXml(order.Id);

            if (existingOrderId.HasValue)
            {
                // Заказ уже существует
                Console.WriteLine($"Заказ с ID {order.Id} уже существует.");
                return existingOrderId.Value;
            }
            else
            {
                // Заказ не найден, добавляем новый
                var command = _unitOfWork.Connection.CreateCommand();
                command.Transaction = _unitOfWork.Transaction;
                command.CommandText = @"
                    INSERT INTO Покупки (ID_Из_XML, ID_Пользователя, Дата_покупки, Общая_сумма)
                    VALUES (@XmlOrderId, @UserId, @Date, @Sum);
                    SELECT SCOPE_IDENTITY();"; // Получаем ID вставленной записи

                // Добавляем параметры
                AddParameter(command, "@XmlOrderId", order.Id);
                AddParameter(command, "@UserId", userId);
                AddParameter(command, "@Date", order.Date);
                AddParameter(command, "@Sum", order.Sum);

                // Получаем ID нового заказа
                int newOrderId = Convert.ToInt32(command.ExecuteScalar());
                Console.WriteLine($"Заказ с ID {order.Id} добавлен.");
                return newOrderId; // Возвращаем новый ID заказа
            }
        }

        // Метод удаления заказа
        public void DeleteOrder(int orderId)
        {
            var command = _unitOfWork.Connection.CreateCommand();
            command.Transaction = _unitOfWork.Transaction;
            command.CommandText = @"
                DELETE FROM Покупки_Товаров WHERE ID_Покупки = @OrderId;
                DELETE FROM Покупки WHERE ID_Покупки = @OrderId;";

            AddParameter(command, "@OrderId", orderId);
            command.ExecuteNonQuery(); // Выполнение команды удаления
        }

        // Метод получения существующего ID заказа
        public int? GetOrderId(int orderId)
        {
            var command = _unitOfWork.Connection.CreateCommand();
            command.Transaction = _unitOfWork.Transaction;
            command.CommandText = @"
                SELECT ID_Покупки FROM Покупки WHERE ID_Из_XML = @OrderId";

            AddParameter(command, "@OrderId", orderId);
            var result = command.ExecuteScalar();
            return result != null ? (int?)Convert.ToInt32(result) : null;
        }

        public int? GetOrderIdFromXml(int xmlOrderId)
        {
            var command = _unitOfWork.Connection.CreateCommand();
            command.Transaction = _unitOfWork.Transaction;
            command.CommandText = "SELECT ID_Покупки FROM Покупки WHERE ID_Из_XML = @XmlOrderId";

            AddParameter(command, "@XmlOrderId", xmlOrderId);
            var result = command.ExecuteScalar();
            return result != null ? (int?)Convert.ToInt32(result) : null;
        }

        // Метод обновления заказа
        public void UpdateOrder(Order order, int userId)
        {
            var command = _unitOfWork.Connection.CreateCommand();
            command.Transaction = _unitOfWork.Transaction;
            command.CommandText = @"
            UPDATE Покупки
            SET ID_Пользователя = @UserId,
                Дата_покупки = @Date,
                Общая_сумма = @Sum
            WHERE ID_Из_XML = @XmlOrderId";

            AddParameter(command, "@XmlOrderId", order.Id);
            AddParameter(command, "@UserId", userId);
            AddParameter(command, "@Date", order.Date);
            AddParameter(command, "@Sum", order.Sum);

            command.ExecuteNonQuery();
            Console.WriteLine($"Заказ с ID {order.Id} обновлен.");
        }

        // Вспомогательный метод для добавления параметров в команду
        private void AddParameter(IDbCommand command, string parameterName, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            command.Parameters.Add(param);
        }
    }
}
