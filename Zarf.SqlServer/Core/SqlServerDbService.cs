using System.Data;
using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbService : IDbService
    {
        protected virtual string ConnectionString { get; set; }

        public IDbConnection GetDbConnection(string connectionString = "")
        {
            return new SqlConnection(string.IsNullOrEmpty(connectionString) ? ConnectionString : connectionString);
        }

        public IDbCommand GetDbCommand()
        {
            return GetDbCommand(GetDbConnection());
        }

        public IDbCommand GetDbCommand(IDbConnection dbConnection)
        {
            return GetDbCommand(dbConnection, null);
        }

        public IDbCommand GetDbCommand(IDbConnection dbConnection, IDbTransaction dbTransaction)
        {
            return new SqlCommand()
            {
                Connection = (SqlConnection)dbConnection,
                Transaction = (SqlTransaction)dbTransaction
            };
        }

        public IDbTransaction GetDbTransaction(IDbConnection dbConnection, IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            return dbConnection.BeginTransaction(isolationLevel);
        }

        public void SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
