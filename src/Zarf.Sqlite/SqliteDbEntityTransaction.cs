using System;
using Zarf.Core;

namespace Zarf.Sqlite
{
    internal class SqliteDbEntityTransaction : IDbEntityTransaction
    {
        private int _transOrder;

        private SqliteDbEntityConnection _sqlEnittyConnection;

        private bool _completed;

        public Guid Id { get; }

        internal SqliteDbEntityTransaction(SqliteDbEntityConnection sqlEntityConnection, int transOrder)
        {
            _transOrder = transOrder;
            _sqlEnittyConnection = sqlEntityConnection;
            Id = Guid.NewGuid();
        }

        public void Commit()
        {
            if (!_completed)
            {
                _completed = true;
                _sqlEnittyConnection.CommitTransaction(_transOrder);
            }
        }

        public void Rollback()
        {
            if (!_completed)
            {
                _completed = true;
                _sqlEnittyConnection.RollbackTransaction(_transOrder);
            }
        }

        public void Dispose()
        {
            Commit();
        }
    }
}
