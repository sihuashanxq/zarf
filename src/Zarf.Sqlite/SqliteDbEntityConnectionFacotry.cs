using System.Data.SQLite;
using Zarf.Core;

namespace Zarf.Sqlite
{
    internal class SqliteDbEntityConnectionFacotry : IDbEntityConnectionFacotry
    {
        private string _connectionString;

        internal SqliteDbEntityConnectionFacotry(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbEntityConnection Create()
        {
            return Create(_connectionString);
        }

        public IDbEntityConnection Create(string connectionString)
        {
            return new SqliteDbEntityConnection(new SQLiteConnection(connectionString));
        }
    }
}
