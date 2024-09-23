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

        // Сохранение пользователя (уже реализован)
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

        // Обновление данных пользователя (уже реализован)
        public void UpdateUser(User user, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                //command.CommandText = @"
                //    UPDATE Пользователи 
                //    SET Электронная_почта = @Email 
                //    WHERE Имя_пользователя = @FullName";

                command.CommandText = @"
                    UPDATE Пользователи 
                    SET  Имя_пользователя = @FullName
                    WHERE Электронная_почта = @Email ";

                var fullNameParam = command.CreateParameter();
                fullNameParam.ParameterName = "@FullName";
                fullNameParam.Value = user.FullName;
                command.Parameters.Add(fullNameParam);

                var emailParam = command.CreateParameter();
                emailParam.ParameterName = "@Email";
                emailParam.Value = user.Email;
                command.Parameters.Add(emailParam);

                command.ExecuteNonQuery();
            }
        }

        // Новый метод для получения ID пользователя
        public int? GetUserId(User user, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
            SELECT ID_Пользователя 
            FROM Пользователи 
            WHERE Электронная_почта = @Email";

                var emailParam = command.CreateParameter();
                emailParam.ParameterName = "@Email";
                emailParam.Value = user.Email;
                command.Parameters.Add(emailParam);

               
                var result = command.ExecuteScalar();
                return result != null ? (int?)result : null;
            }
        }


        // Новый метод для удаления пользователя
        public void DeleteUser(int userId, IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "DELETE FROM Пользователи WHERE ID_Пользователя = @UserId";

                var userIdParam = command.CreateParameter();
                userIdParam.ParameterName = "@UserId";
                userIdParam.Value = userId;
                command.Parameters.Add(userIdParam);

                command.ExecuteNonQuery();
            }
        }
    }
}
