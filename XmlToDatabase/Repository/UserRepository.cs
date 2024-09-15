using System.Data;
using Microsoft.Data.SqlClient;

namespace XmlToDatabase
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public UserRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int SaveUser(User user, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM Пользователи WHERE Электронная_почта = @Email)
                    BEGIN
                        INSERT INTO Пользователи (Имя_пользователя, Электронная_почта, Имя, Фамилия)
                        VALUES (@FullName, @Email, @FirstName, @LastName);
                    END;
                    SELECT ID_Пользователя FROM Пользователи WHERE Электронная_почта = @Email;";

                var fullNameParts = user.FullName.Split(' ');
                command.Parameters.Add(new SqlParameter("@FullName", user.FullName));
                command.Parameters.Add(new SqlParameter("@Email", user.Email));
                command.Parameters.Add(new SqlParameter("@FirstName", fullNameParts.Length > 1 ? fullNameParts[1] : ""));
                command.Parameters.Add(new SqlParameter("@LastName", fullNameParts.Length > 0 ? fullNameParts[0] : ""));

                return (int)command.ExecuteScalar();
            }
        }
    }
}
