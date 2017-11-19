using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbEntityConnectionFacotry : IDbEntityConnectionFacotry
    {
        private IDbEntityConnection _dbConnection;

        public SqlServerDbEntityConnectionFacotry(string connectionString)
        {
            _dbConnection = new DbEntityConnection(new SqlConnection(connectionString));
        }

        public IDbEntityConnection Create()
        {
            return _dbConnection;
        }
    }
}
