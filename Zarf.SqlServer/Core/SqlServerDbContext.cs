using Zarf.SqlServer.Builders;
using Zarf.SqlServer.Core;
using Zarf.Update.Compilers;
using Zarf.Update.Executors;

namespace Zarf
{
    public class SqlServerDbContext : DbContext
    {
        public SqlServerDbContext(string connectionString)
        {
            DbService = new SqlServerDbService(connectionString);
            Command = new SqlServerDbCommandFacade(DbService);
            DataBase = new SqlServerDataBaseFacade(Command);
            Executor = new CompisteDbCommandExecutor(DataBase, new SqlServerTextBuilder(), new CompositeModifyOperationCompiler());
        }
    }
}
