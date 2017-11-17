using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbConnectionFacotry : IDbConnectionFacotry
    {
        private IDbConnectionWrapper _dbConnection;

        public SqlServerDbConnectionFacotry(string connectionString)
        {
            _dbConnection = new DbConnectionWrapper(new SqlConnection(connectionString));
        }

        public IDbConnectionWrapper Create()
        {
            return _dbConnection;
        }
    }
}
