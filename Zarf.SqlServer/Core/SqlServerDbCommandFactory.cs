using System.Data;
using System.Data.SqlClient;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{

    public class SqlServerDbCommandFactory : IDbCommandFacotry
    {
        private IDbConnectionFacotry _connectionFacotry;

        public SqlServerDbCommandFactory(IDbConnectionFacotry connectionFacotry)
        {
            _connectionFacotry = connectionFacotry;
        }

        public IDbCommandWrapper Create(IDbConnectionWrapper dbConnection)
        {
            return Create(dbConnection.DbConnection);
        }

        public IDbCommandWrapper Create()
        {
            return Create(_connectionFacotry.Create());
        }

        public IDbCommandWrapper Create(IDbConnection dbConnection)
        {
            return new DbCommandWrapper(new SqlCommand()
            {
                Connection = (SqlConnection)dbConnection
            });
        }
    }
}
