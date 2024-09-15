using System.Data;

namespace XmlToDatabase
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection();  
    }
}
