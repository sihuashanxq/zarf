using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    internal class SqlServerDbEntityConnectionFacotry : IDbEntityConnectionFacotry
    {
        private SqlServerDbEntityConnection _dbConnection;

        internal SqlServerDbEntityConnectionFacotry(string connectionString)
        {
            _dbConnection = new SqlServerDbEntityConnection(new SqlConnection(connectionString));
        }

        public IDbEntityConnection Create()
        {
            return _dbConnection;
        }
    }
}
