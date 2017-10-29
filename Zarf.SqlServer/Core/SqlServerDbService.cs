using System.Data;
using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbService : IDbService
    {
        public IDbCommand CreateDbCommand()
        {
            return CreateDbCommand(null);
        }

        public IDbCommand CreateDbCommand(IDbConnection connection)
        {
            return CreateDbCommand(connection, null);
        }

        public IDbCommand CreateDbCommand(IDbConnection connection, IDbTransaction transaction)
        {
            return new SqlCommand()
            {
                Connection = (SqlConnection)connection,
                Transaction = (SqlTransaction)transaction
            };
        }

        public IDbConnection CreateDbConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public IDbTransaction CreateDbTransaction(IDbConnection connection, IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            return connection.BeginTransaction(isolationLevel);
        }
    }
}
