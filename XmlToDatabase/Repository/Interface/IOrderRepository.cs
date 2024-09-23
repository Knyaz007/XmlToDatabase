using System.Data;

namespace XmlToDatabase
{
    public interface IOrderRepository
    {
        int SaveOrder(Order order, int userId, IDbConnection connection, IDbTransaction transaction);

        // Метод для удаления заказа
        void DeleteOrder(int orderId, IDbConnection connection, IDbTransaction transaction);

        void UpdateOrder(Order order, int userId, IDbConnection connection, IDbTransaction transaction);

        // Метод для получения ID заказа
        int? GetOrderId(int orderId, IDbConnection connection, IDbTransaction transaction);
    }
}
