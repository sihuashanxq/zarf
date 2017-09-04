using System.Data;
using System.Data.SqlClient;

namespace Zarf.SqlServer
{
    public class SqlServerDbService : IDbService
    {
        public IDbCommand CreateDbCommand()
        {
            return new SqlCommand();
        }

        public IDbConnection CreateDbConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public IDbTransaction CreateDbTransaction()
        {
            return null;
        }
    }
}
