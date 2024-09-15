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

        public int SaveOrder(Order order, int userId, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction; 
                command.CommandText = @"
                    INSERT INTO Покупки (ID_Пользователя, Дата_покупки, Общая_сумма)
                    VALUES (@UserId, @Date, @Sum);
                    SELECT SCOPE_IDENTITY();"; 

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

                return Convert.ToInt32(command.ExecuteScalar()); 
            }
        }
    }
}
