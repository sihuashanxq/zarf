using System;
using System.Data;
using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer
{
    internal class SqlServerDbEntityConnection : DbEntityConnection
    {
        internal SqlConnection Connection => DbConnection as SqlConnection;

        internal SqlTransaction Transaction { get; private set; }

        private int _transCount = 0;

        const string SavePointPrefix = "TRANSACTION";

        internal SqlServerDbEntityConnection(SqlConnection sqlConnection)
            : base(sqlConnection)
        {

        }

        public override IDbEntityTransaction BeginTransaction(IsolationLevel level)
        {
            var savePoint = CreateTransactinSavePoint();
            if (Transaction == null)
            {
                Open();
                Transaction = Connection.BeginTransaction(savePoint);
            }
            else
            {
                Transaction.Save(savePoint);
            }

            return new SqlServerDbEntityTransaction(this, _transCount, savePoint);
        }

        public override bool HasTransaction()
        {
            return Transaction != null;
        }

        internal void RollbackTransaction(int transCount, string savePoint)
        {
            if (!HasTransaction())
            {
                throw new Exception("there not exists a activing transaction!");
            }

            Transaction.Rollback(savePoint);
            if (transCount == 1)
            {
                Close();
                Transaction = null;
                _transCount = 0;
            }
        }

        internal void CommitTransaction(int transCount, string savePoint)
        {
            if (!HasTransaction())
            {
                throw new Exception("there not exists a activing transaction!");
            }

            if (transCount == 1)
            {
                Transaction.Commit();
                Close();
                Transaction = null;
                _transCount = 0;
            }
        }

        private string CreateTransactinSavePoint()
        {
            return SavePointPrefix + (++_transCount);
        }
    }
}
