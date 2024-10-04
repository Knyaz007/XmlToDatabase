using System.Data;
using Microsoft.Data.SqlClient;
using XmlToDatabase.Utils;

namespace XmlToDatabase
{
    public class UserRepository : IUserRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Сохранение пользователя
        public int SaveUser(User user)
        {
            using (var command = _unitOfWork.Connection.CreateCommand())
            {
                command.Transaction = _unitOfWork.Transaction;
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

        // Обновление данных пользователя
        public void UpdateUser(User user)
        {
            using (var command = _unitOfWork.Connection.CreateCommand())
            {
                command.Transaction = _unitOfWork.Transaction;
                command.CommandText = @"
                    UPDATE Пользователи 
                    SET Имя_пользователя = @FullName
                    WHERE Электронная_почта = @Email;";

                command.Parameters.Add(new SqlParameter("@FullName", user.FullName));
                command.Parameters.Add(new SqlParameter("@Email", user.Email));

                command.ExecuteNonQuery();
            }
        }

        // Получение ID пользователя
        public int? GetUserId(User user)
        {
            using (var command = _unitOfWork.Connection.CreateCommand())
            {
                command.Transaction = _unitOfWork.Transaction;
                command.CommandText = @"
                    SELECT ID_Пользователя 
                    FROM Пользователи 
                    WHERE Электронная_почта = @Email;";

                command.Parameters.Add(new SqlParameter("@Email", user.Email));

                var result = command.ExecuteScalar();
                return result != null ? (int?)result : null;
            }
        }

        // Удаление пользователя
        public void DeleteUser(int userId)
        {
            using (var command = _unitOfWork.Connection.CreateCommand())
            {
                command.Transaction = _unitOfWork.Transaction;
                command.CommandText = "DELETE FROM Пользователи WHERE ID_Пользователя = @UserId;";

                command.Parameters.Add(new SqlParameter("@UserId", userId));

                command.ExecuteNonQuery();
            }
        }
    }
}
