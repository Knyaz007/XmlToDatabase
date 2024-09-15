using System.Data;
using Microsoft.Data.SqlClient;
using Xunit;

namespace XmlToDatabase.Tests
{
    public class DatabaseConnectionFactoryTests
    {
        [Fact]
        public void CreateConnection_ShouldReturnIDbConnection()
        {
            // Arrange          
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename=C:\Users\y-men\Downloads\XmlToDatabase\XmlToDatabase\purchase_sale.mdf; Integrated Security=True;";

            var factory = new DatabaseConnectionFactory(connectionString);

            // Act
            var connection = factory.CreateConnection();

            // Assert
            Assert.NotNull(connection);
            Assert.IsAssignableFrom<IDbConnection>(connection); 

            // Assert
            Assert.NotNull(connection);
            Assert.IsType<SqlConnection>(connection);
            Assert.Equal(connectionString, ((SqlConnection)connection).ConnectionString);


        }
    }
}
