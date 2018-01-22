using Microsoft.Extensions.DependencyInjection;
using Zarf.Core;

namespace Zarf.SqlServer
{
    public class SqlServerDbServiceBuilder : DbServiceBuilder
    {
        public override IDbService BuildService(string connectionString, IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IDbEntityConnectionFacotry>(
                p => new SqlServerDbEntityConnectionFacotry(connectionString));

            return new DbService(
                connectionString,
                serviceCollection.BuildServiceProvider());
        }

        public override IDbService BuildService(string connectionString)
        {
            return BuildService(connectionString, new ServiceCollection().AddSqlServer());
        }
    }
}
