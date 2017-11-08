using Microsoft.Extensions.DependencyInjection;
using Zarf.Builders;
using Zarf.Core;
using Zarf.SqlServer.Builders;
using Zarf.SqlServer.Extensions;
using Zarf.Update.Compilers;
using Zarf.Update.Executors;
using Zarf.SqlServer.Core;

namespace Zarf
{
    public class SqlServerDbContextParts : IDbContextParts
    {
        public ISqlTextBuilder SqlBuilder { get; }

        public IDbCommandFacotry CommandFacotry { get; }

        public IDbConnectionFacotry ConnectionFacotry { get; }

        public SqlServerDbContextParts(string connectionString)
        {
            SqlBuilder = new SqlServerTextBuilder();
            ConnectionFacotry = new SqlServerDbConnectionFacotry(connectionString);
            CommandFacotry = new SqlServerDbCommandFactory(ConnectionFacotry);
        }
    }

    public class SqlServerDbContext : DbContext
    {
        public SqlServerDbContext(string connectionString)
        {
            DbContextParts = new SqlServerDbContextParts(connectionString);

            ServiceProvider = new ServiceCollection().AddSqlServer().BuildServiceProvider();
            DbService = ServiceProvider.GetService<IDbService>();
            DbService.SetConnectionString(connectionString);
            Command = ServiceProvider.GetService<IDbCommandFacade>();
            DataBase = ServiceProvider.GetService<IDataBaseFacade>();
            Executor = new CompisteDbCommandExecutor(DataBase, ServiceProvider.GetService<ISqlTextBuilder>(), new CompositeModifyOperationCompiler());
        }
    }
}
