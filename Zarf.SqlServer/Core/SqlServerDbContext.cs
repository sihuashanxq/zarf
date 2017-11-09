using Zarf.SqlServer.Core;

namespace Zarf
{
    public class SqlServerDbContext : DbContext
    {
        public SqlServerDbContext(string connectionString)
            : base(new SqlServerDbContextParts(connectionString))
        {

        }
    }
}
