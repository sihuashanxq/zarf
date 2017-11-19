using System;
using System.Data;

namespace Zarf.Core
{
    public class DbEntityConnection : IDbEntityConnection, IDbEntityTransaction
    {
        private IDbTransaction _transaction;

        public string ConnectionString => DbConnection.ConnectionString;

        public IDbConnection DbConnection { get; }

        public IDbTransaction DbTransaction => _transaction;

        public Guid Id { get; }

        public DbEntityConnection(IDbConnection dbConnection)
        {
            DbConnection = dbConnection;
            Id = Guid.NewGuid();
        }

        public IDbEntityTransaction BeginTransaction(IsolationLevel level)
        {
            if (_transaction == null)
            {
                Open();
                _transaction = DbConnection.BeginTransaction(level);
            }

            return this;
        }

        public void Rollback()
        {
            if (_transaction == null)
            {
                throw new NotSupportedException("not exists a transaction!");
            }

            DbTransaction.Rollback();
            Close();
        }

        public void Commit()
        {
            if (DbTransaction == null)
            {
                throw new NotSupportedException("not exists a transaction!");
            }

            DbTransaction.Commit();
            Close();
        }

        public void Open()
        {
            if (DbConnection.State != ConnectionState.Closed)
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

            if (_transaction != null)
            {
                _transaction = null;
            }
        }
    }
}
