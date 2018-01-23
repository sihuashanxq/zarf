using System;
using System.Data;
using System.Data.SQLite;
using Zarf.Core;

namespace Zarf.Sqlite
{
    internal class SqliteDbEntityConnection : DbEntityConnection
    {
        internal SQLiteConnection Connection => DbConnection as SQLiteConnection;

        internal SQLiteTransaction Transaction { get; private set; }

        private int _transCount = 0;

        internal SqliteDbEntityConnection(SQLiteConnection connection)
            : base(connection)
        {

        }

        public override IDbEntityTransaction BeginTransaction(IsolationLevel level)
        {
            if (Transaction == null)
            {
                Open();

                Transaction = Connection.BeginTransaction();
            }

            _transCount++;

            return new SqliteDbEntityTransaction(this, _transCount);
        }

        public override bool HasTransaction()
        {
            return Transaction != null;
        }

        internal void RollbackTransaction(int transCount)
        {
            if (!HasTransaction())
            {
                throw new Exception("there not exists a activing transaction!");
            }

            if (transCount == 1)
            {
                Transaction.Rollback();
                Close();
                Transaction = null;
                _transCount = 0;
            }
        }

        internal void CommitTransaction(int transCount)
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
    }
}
