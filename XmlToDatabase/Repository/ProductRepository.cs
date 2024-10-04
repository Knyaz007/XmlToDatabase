using System;
using System.Data;
using XmlToDatabase.Utils;

namespace XmlToDatabase
{
    public class ProductRepository : IProductRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //public void SaveProduct(Product product, int orderId)
        //{
        //    using (var command = _unitOfWork.Connection.CreateCommand())
        //    {
        //        _unitOfWork.Connection.Open();
        //        using (var transaction = _unitOfWork.Connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                SaveProduct(product, orderId);
        //                transaction.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                Console.WriteLine($"Ошибка при работе с базой данных: {ex.Message}");
        //            }
        //        }
        //    }
        //}

        public void SaveProduct(Product product, int orderId)
        {
            if (ProductExists(product.Name))
            { 
                // Удаляем продукт, если существует
                DeleteProduct(product.Name);
            }

            using (var command = _unitOfWork.Connection.CreateCommand())
            {
                command.Transaction = _unitOfWork.Transaction;
                command.CommandText = @"
                    INSERT INTO Товары (Название, Цена, Количество_на_складе)
                    VALUES (@Name, @Price, 0);
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

        // Метод для проверки существования продукта
        public bool ProductExists(string productName)
        {
            using (var command = _unitOfWork.Connection.CreateCommand())
            {
                command.Transaction = _unitOfWork.Transaction;
                command.CommandText = "SELECT COUNT(1) FROM Товары WHERE Название = @Name";

                var nameParam = command.CreateParameter();
                nameParam.ParameterName = "@Name";
                nameParam.Value = productName;
                command.Parameters.Add(nameParam);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        // Метод для удаления продукта
        public void DeleteProduct(string productName)
        {
            using (var command = _unitOfWork.Connection.CreateCommand())
            {
                command.Transaction = _unitOfWork.Transaction;
                command.CommandText = "DELETE FROM Товары WHERE Название = @Name";

                var nameParam = command.CreateParameter();
                nameParam.ParameterName = "@Name";
                nameParam.Value = productName;
                command.Parameters.Add(nameParam);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateProduct(Product product)
        {
            using (var command = _unitOfWork.Connection.CreateCommand())
            {
                command.Transaction = _unitOfWork.Transaction;
                command.CommandText = @"
                UPDATE Товары 
                SET Цена = @Price 
                WHERE Название = @Name";

                var nameParam = command.CreateParameter();
                nameParam.ParameterName = "@Name";
                nameParam.Value = product.Name;
                command.Parameters.Add(nameParam);

                var priceParam = command.CreateParameter();
                priceParam.ParameterName = "@Price";
                priceParam.Value = product.Price;
                command.Parameters.Add(priceParam);

                command.ExecuteNonQuery();
            }
        }
    }
}
