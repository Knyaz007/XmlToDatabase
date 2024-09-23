using System.Data;

namespace XmlToDatabase
{
    public interface IUserRepository
    {
        // Метод для сохранения пользователя
        int SaveUser(User user, IDbConnection connection, IDbTransaction transaction);

        // Метод для получения ID пользователя по его данным
        int? GetUserId(User user, IDbConnection connection, IDbTransaction transaction);

        // Метод для удаления пользователя по его ID
        void DeleteUser(int userId, IDbConnection connection, IDbTransaction transaction);

        void UpdateUser(User user, IDbConnection connection, IDbTransaction transaction);
    }
}
