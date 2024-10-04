using System.Data;
using Moq;
using Xunit;
using XmlToDatabase.Utils;

namespace XmlToDatabase.Tests
{
    public class UserRepositoryTests
    {
        [Fact]
        public void SaveUser_ShouldInsertUser_WhenUserDoesNotExist()
        {
            // Arrange
            var mockConnection = new Mock<IDbConnection>();
            var mockTransaction = new Mock<IDbTransaction>();
            var mockCommand = new Mock<IDbCommand>();
            var mockParameterCollection = new Mock<IDataParameterCollection>();

            mockCommand.Setup(cmd => cmd.Parameters).Returns(mockParameterCollection.Object);
            mockConnection.Setup(conn => conn.CreateCommand()).Returns(mockCommand.Object);
            mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(1);

            // Настройка моков для IUnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.Connection).Returns(mockConnection.Object);
            mockUnitOfWork.Setup(u => u.Transaction).Returns(mockTransaction.Object);

            var userRepository = new UserRepository(mockUnitOfWork.Object);

            var user = new User
            {
                FullName = "John Doe",
                Email = "john.doe@example.com"
            };

            // Act
            var result = userRepository.SaveUser(user);

            // Assert
            Assert.Equal(1, result);

            mockConnection.Verify(conn => conn.CreateCommand(), Times.Once);

            mockCommand.VerifySet(cmd => cmd.CommandText = It.Is<string>(s =>
                s.Contains("INSERT INTO Пользователи") &&
                s.Contains("@FullName") &&
                s.Contains("@Email") &&
                s.Contains("@FirstName") &&
                s.Contains("@LastName")), Times.Once);

            mockCommand.Verify(cmd => cmd.ExecuteScalar(), Times.Once);

            mockParameterCollection.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@FullName" && param.Value.ToString() == "John Doe")), Times.Once);

            mockParameterCollection.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@Email" && param.Value.ToString() == "john.doe@example.com")), Times.Once);

            mockParameterCollection.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@FirstName" && param.Value.ToString() == "Doe")), Times.Once);

            mockParameterCollection.Verify(p => p.Add(It.Is<IDbDataParameter>(param =>
                param.ParameterName == "@LastName" && param.Value.ToString() == "John")), Times.Once);

            // Commit the transaction
            mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }
    }
}
