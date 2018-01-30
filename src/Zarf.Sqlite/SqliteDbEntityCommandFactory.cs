using System.Data.SQLite;
using Zarf.Core;

namespace Zarf.Sqlite
{
    internal class SqliteDbEntityCommandFactory : IDbEntityCommandFacotry
    {
        private IDbEntityConnectionFacotry _entityConnectionFacotry;

        internal SqliteDbEntityCommandFactory(IDbEntityConnectionFacotry entityConnectionFacotry)
        {
            _entityConnectionFacotry = entityConnectionFacotry;
        }

        public IDbEntityCommand Create(IDbEntityConnection entityConnection)
        {
            return new SqliteDbEntityCommand(new SQLiteCommand(), entityConnection);
        }

        public IDbEntityCommand Create()
        {
            return Create(_entityConnectionFacotry.Create());
        }

        public IDbEntityCommand Create(string connectionString)
        {
            return Create(_entityConnectionFacotry.Create(connectionString));
        }
    }
}
