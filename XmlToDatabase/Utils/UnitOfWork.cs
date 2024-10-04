using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlToDatabase.Utils
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private IDbTransaction _transaction;

        public IDbConnection Connection { get; private set; }
        public IDbTransaction Transaction => _transaction;

        public UnitOfWork(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            Connection = _connectionFactory.CreateConnection();
            Connection.Open();
        }

        public void BeginTransaction()
        {
            _transaction = Connection.BeginTransaction();
        }

        public void Commit()
        {
            _transaction?.Commit();
            Dispose();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            Dispose();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            Connection?.Dispose();
        }
    }

}
