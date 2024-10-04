using System.Data;

namespace XmlToDatabase
{
    public interface IProductRepository
    {
        // Сохранение продукта без явной передачи соединения и транзакции
        void SaveProduct(Product product, int orderId);

        //// Сохранение продукта с явной передачей соединения и транзакции
        //void SaveProduct(Product product, int orderId, IDbConnection connection, IDbTransaction transaction);

        // Проверка наличия продукта в базе данных
        bool ProductExists(string productName );

        // Удаление продукта из базы данных
        void DeleteProduct(string productName );

        void UpdateProduct(Product product );
    }
}
