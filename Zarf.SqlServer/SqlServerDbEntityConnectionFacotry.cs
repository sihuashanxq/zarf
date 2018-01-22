using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer
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

        public IDbEntityConnection Create(string connectionString)
        {
            return new SqlServerDbEntityConnection(new SqlConnection(connectionString));
        }
    }
}
