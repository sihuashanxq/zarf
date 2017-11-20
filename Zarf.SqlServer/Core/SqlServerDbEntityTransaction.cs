using System;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    internal class SqlServerDbEntityTransaction : IDbEntityTransaction
    {
        private int _transOrder;

        private string _savePoint;

        private SqlServerDbEntityConnection _sqlEnittyConnection;

        private bool _completed;

        public Guid Id { get; }

        internal SqlServerDbEntityTransaction(
            SqlServerDbEntityConnection sqlEntityConnection, int transOrder, string savePoint
        )
        {
            _transOrder = transOrder;
            _savePoint = savePoint;
            _sqlEnittyConnection = sqlEntityConnection;
            Id = Guid.NewGuid();
        }

        public void Commit()
        {
            if (!_completed)
            {
                _completed = true;
                _sqlEnittyConnection.CommitTransaction(_transOrder, _savePoint);
            }
        }

        public void Rollback()
        {
            if (!_completed)
            {
                _completed = true;
                _sqlEnittyConnection.RollbackTransaction(_transOrder, _savePoint);
            }
        }

        public void Dispose()
        {
            Commit();
        }
    }
}
