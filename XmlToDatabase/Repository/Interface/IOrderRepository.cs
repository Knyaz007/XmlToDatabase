using System.Data;

namespace XmlToDatabase
{
    public interface IOrderRepository
    {
        int SaveOrder(Order order, int userId, IDbConnection connection, IDbTransaction transaction);
    }
}
