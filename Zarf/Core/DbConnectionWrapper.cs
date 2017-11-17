using System;
using System.Data;

namespace Zarf.Core
{
    public class DbConnectionWrapper : IDbConnectionWrapper
    {
        public string ConnectionString => DbConnection.ConnectionString;

        public IDbConnection DbConnection { get; }

        public IDbTransaction CurrentTransaction { get; protected set; }

        public DbConnectionWrapper(IDbConnection dbConnection)
        {
            DbConnection = dbConnection;
        }

        public IDbTransactionWrapper BeginTransaction()
        {
            Open();
            CurrentTransaction = DbConnection.BeginTransaction();
            return new DbTransactionWrapper(Guid.NewGuid(), this);
        }

        public void RollbackTransaction()
        {
            if (CurrentTransaction == null)
            {
                throw new Exception("none transaction!");
            }

            CurrentTransaction.Rollback();
            CurrentTransaction = null;
            Close();
        }

        public void CommitTransaction()
        {
            if (CurrentTransaction == null)
            {
                throw new Exception("none transaction!");
            }

            CurrentTransaction.Commit();
            CurrentTransaction = null;
            Close();
        }

        public void Open()
        {
            if (DbConnection.State == ConnectionState.Closed)
            {
                DbConnection.Open();
            }
        }

        public void Close()
        {
            if (DbConnection.State == ConnectionState.Open)
            {
                DbConnection.Close();
            }
        }
    }
}
