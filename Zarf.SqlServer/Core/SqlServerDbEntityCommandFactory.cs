using System.Data;
using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbEntityCommandFactory : IDbEntityCommandFacotry
    {
        private IDbEntityConnectionFacotry _entityConnectionFacotry;

        public SqlServerDbEntityCommandFactory(IDbEntityConnectionFacotry entityConnectionFacotry)
        {
            _entityConnectionFacotry = entityConnectionFacotry;
        }

        public IDbEntityCommand Create(IDbEntityConnection entityConnection)
        {
            return new DbEntityCommand(new SqlCommand(), entityConnection);
        }

        public IDbEntityCommand Create()
        {
            return Create(_entityConnectionFacotry.Create());
        }
    }
}
