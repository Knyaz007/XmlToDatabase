using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace XmlToDatabase
{
    public interface IUserRepository
    {
        int SaveUser(User user, IDbConnection connection, IDbTransaction transaction);
    }


}
