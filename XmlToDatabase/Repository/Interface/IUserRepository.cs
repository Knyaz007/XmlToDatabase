using System.Data;

namespace XmlToDatabase
{
    public interface IUserRepository
    {
        // Метод для сохранения пользователя
        int SaveUser(User user );

        // Метод для получения ID пользователя по его данным
        int? GetUserId(User user );

        // Метод для удаления пользователя по его ID
        void DeleteUser(int userId);

        void UpdateUser(User user);
    }
}
