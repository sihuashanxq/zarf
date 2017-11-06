using Microsoft.Extensions.DependencyInjection;
using Zarf.Builders;
using Zarf.Core;
using Zarf.SqlServer.Extensions;
using Zarf.Update.Compilers;
using Zarf.Update.Executors;

namespace Zarf
{
    public class SqlServerDbContext : DbContext
    {
        public SqlServerDbContext(string connectionString)
            : base(connectionString)
        {
            ServiceProvider = new ServiceCollection().AddSqlServer().BuildServiceProvider();
            DbService = ServiceProvider.GetService<IDbService>();
            DbService.SetConnectionString(connectionString);
            Command = ServiceProvider.GetService<IDbCommandFacade>();
            DataBase = ServiceProvider.GetService<IDataBaseFacade>();
            Executor = new CompisteDbCommandExecutor(DataBase, ServiceProvider.GetService<ISqlTextBuilder>(), new CompositeModifyOperationCompiler());
        }
    }
}
