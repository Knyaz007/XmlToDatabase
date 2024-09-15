using System.Data;

namespace XmlToDatabase
{
    public interface IProductRepository
    {
        void SaveProduct(Product product, int orderId);
        void SaveProduct(Product product, int orderId, IDbConnection connection, IDbTransaction transaction);
    }
}
