using System;
using System.Data;

namespace XmlToDatabase
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public ProductRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void SaveProduct(Product product, int orderId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                SaveProduct(product, orderId, connection, null);
            }
        }

        public void SaveProduct(Product product, int orderId, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM Товары WHERE Название = @Name)
                BEGIN
                    INSERT INTO Товары (Название, Цена, Количество_на_складе)
                    VALUES (@Name, @Price, 0);
                END;
                SELECT ID_Товара FROM Товары WHERE Название = @Name;";

                var nameParam = command.CreateParameter();
                nameParam.ParameterName = "@Name";
                nameParam.Value = product.Name;
                command.Parameters.Add(nameParam);

                var priceParam = command.CreateParameter();
                priceParam.ParameterName = "@Price";
                priceParam.Value = product.Price;
                command.Parameters.Add(priceParam);

                int productId = Convert.ToInt32(command.ExecuteScalar());

                command.CommandText = @"
                INSERT INTO Покупки_Товаров (ID_Покупки, ID_Товара, Количество, Общая_стоимость)
                VALUES (@OrderId, @ProductId, @Quantity, @TotalCost);";

                command.Parameters.Clear();

                var orderIdParam = command.CreateParameter();
                orderIdParam.ParameterName = "@OrderId";
                orderIdParam.Value = orderId;
                command.Parameters.Add(orderIdParam);

                var productIdParam = command.CreateParameter();
                productIdParam.ParameterName = "@ProductId";
                productIdParam.Value = productId;
                command.Parameters.Add(productIdParam);

                var quantityParam = command.CreateParameter();
                quantityParam.ParameterName = "@Quantity";
                quantityParam.Value = product.Quantity;
                command.Parameters.Add(quantityParam);

                var totalCostParam = command.CreateParameter();
                totalCostParam.ParameterName = "@TotalCost";
                totalCostParam.Value = product.Price * product.Quantity;
                command.Parameters.Add(totalCostParam);

                command.ExecuteNonQuery();
            }
        }
    }
}
