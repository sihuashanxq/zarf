using Microsoft.Extensions.DependencyInjection;
using Zarf.Core;

namespace Zarf.Sqlite
{
    public class SqliteDbServiceBuilder : DbServiceBuilder
    {
        public override IDbService BuildService(string connectionString, IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IDbEntityConnectionFacotry>(
                p => new SqliteDbEntityConnectionFacotry(connectionString));

            return new DbService(
                connectionString,
                serviceCollection.BuildServiceProvider());
        }

        public override IDbService BuildService(string connectionString)
        {
            return BuildService(connectionString, new ServiceCollection().AddSqlite());
        }
    }
}
