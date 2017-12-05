using System;
using System.Data;
using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    internal class SqlServerDbEntityConnection : DbEntityConnection
    {
        internal SqlConnection SqlConnection => DbConnection as SqlConnection;

        internal SqlTransaction SqlTransaction { get; private set; }

        private int _transCount = 0;

        const string SavePointPrefix = "TRANSACTION";

        internal SqlServerDbEntityConnection(SqlConnection sqlConnection)
            : base(sqlConnection)
        {

        }

        public override IDbEntityTransaction BeginTransaction(IsolationLevel level)
        {
            var savePoint = CreateTransactinSavePoint();
            if (SqlTransaction == null)
            {
                Open();
                SqlTransaction = SqlConnection.BeginTransaction(savePoint);
            }
            else
            {
                SqlTransaction.Save(savePoint);
            }

            return new SqlServerDbEntityTransaction(this, _transCount, savePoint);
        }

        public override bool HasTransaction()
        {
            return SqlTransaction != null;
        }

        internal void RollbackTransaction(int transCount, string savePoint)
        {
            if (!HasTransaction())
            {
                throw new Exception("there not exists a activing transaction!");
            }

            SqlTransaction.Rollback(savePoint);
            if (transCount == 1)
            {
                Close();
                SqlTransaction = null;
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
                SqlTransaction.Commit();
                Close();
                SqlTransaction = null;
                _transCount = 0;
            }
        }

        private string CreateTransactinSavePoint()
        {
            return SavePointPrefix + (++_transCount);
        }
    }
}
