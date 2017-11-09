using System.Data;

namespace Zarf.Core
{
    public class DbConnectionWrapper : IDbConnectionWrapper
    {
        public string ConnectionString => DbConnection.ConnectionString;

        public IDbConnection DbConnection { get; }

        public DbConnectionWrapper(IDbConnection dbConnection)
        {
            DbConnection = dbConnection;
        }
    }
}
