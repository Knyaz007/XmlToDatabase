using System.Data;

namespace XmlToDatabase
{
    public interface IOrderRepository
    {
        int SaveOrder(Order order, int userId); // Без необходимости в явной передаче соединений и транзакций

        // Метод для удаления заказа
        void DeleteOrder(int orderId);

        void UpdateOrder(Order order, int userId);

        // Метод для получения ID заказа
        int? GetOrderId(int orderId);
    }
}
