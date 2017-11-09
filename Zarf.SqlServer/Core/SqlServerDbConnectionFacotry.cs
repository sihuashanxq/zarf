using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbConnectionFacotry : IDbConnectionFacotry
    {
        private string _connectionString;

        public SqlServerDbConnectionFacotry(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnectionWrapper Create()
        {
            return new DbConnectionWrapper(new SqlConnection(_connectionString));
        }
    }
}
