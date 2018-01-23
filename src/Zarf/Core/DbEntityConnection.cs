using System.Data;

namespace Zarf.Core
{
    public abstract class DbEntityConnection : IDbEntityConnection
    {
        public string ConnectionString => DbConnection.ConnectionString;

        public IDbConnection DbConnection { get; }

        public DbEntityConnection(IDbConnection dbConnection)
        {
            DbConnection = dbConnection;
        }

        public abstract IDbEntityTransaction BeginTransaction(IsolationLevel level);

        public abstract bool HasTransaction();

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

        public virtual void Dispose()
        {
            DbConnection.Dispose();
        }
    }
}
