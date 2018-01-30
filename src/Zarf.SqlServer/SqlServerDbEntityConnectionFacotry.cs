using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer
{
    internal class SqlServerDbEntityConnectionFacotry : IDbEntityConnectionFacotry
    {
        private string _connectionString;

        private IDbEntityConnection _scopedConnection;

        internal SqlServerDbEntityConnectionFacotry(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbEntityConnection Create()
        {
            return Create(_connectionString);
        }

        public IDbEntityConnection Create(string connectionString)
        {
            return new SqlServerDbEntityConnection(new SqlConnection(connectionString));
        }

        public IDbEntityConnection CreateDbContextScopedConnection()
        {
            lock (this)
            {
                if (_scopedConnection == null)
                {
                    _scopedConnection = Create();
                }

                return _scopedConnection;
            }
        }
    }
}
