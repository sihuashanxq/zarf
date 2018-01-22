using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer
{
    internal class SqlServerDbEntityCommandFactory : IDbEntityCommandFacotry
    {
        private IDbEntityConnectionFacotry _entityConnectionFacotry;

        internal SqlServerDbEntityCommandFactory(IDbEntityConnectionFacotry entityConnectionFacotry)
        {
            _entityConnectionFacotry = entityConnectionFacotry;
        }

        public IDbEntityCommand Create(IDbEntityConnection entityConnection)
        {
            return new SqlServerDbEntityCommand(new SqlCommand(), entityConnection);
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
