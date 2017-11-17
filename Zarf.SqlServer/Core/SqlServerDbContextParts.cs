using Zarf.Builders;
using Zarf.Core;
using Zarf.SqlServer.Builders;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDbContextParts : IDbContextParts
    {
        public ISqlTextBuilder SqlBuilder { get; }

        public IDbCommandFacotry CommandFacotry { get; }

        public IDbConnectionFacotry ConnectionFacotry { get; }

        public IDbConnectionWrapper DbConnection { get; }

        public SqlServerDbContextParts(string connectionString)
        {
            SqlBuilder = new SqlServerTextBuilder();
            ConnectionFacotry = new SqlServerDbConnectionFacotry(connectionString);
            CommandFacotry = new SqlServerDbCommandFactory(ConnectionFacotry);
            DbConnection = ConnectionFacotry.Create();
        }
    }
}
