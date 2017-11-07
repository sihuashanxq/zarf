using Microsoft.Extensions.DependencyInjection;
using Zarf.Builders;
using Zarf.SqlServer.Builders;
using Zarf.Core;
using Zarf.SqlServer.Core;

namespace Zarf.SqlServer.Extensions
{
    public static class SqlServerDbServiceExtension
    {
        public static IServiceCollection AddSqlServer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISqlTextBuilder, SqlServerTextBuilder>();
            serviceCollection.AddSingleton<IDbService, SqlServerDbService>();
            serviceCollection.AddSingleton<IDbCommandFacade, SqlServerDbCommandFacade>((serviceProvider) => new SqlServerDbCommandFacade(serviceProvider.GetService<IDbService>()));
            serviceCollection.AddSingleton<IDataBaseFacade, SqlServerDataBaseFacade>((serviceProvider) => new SqlServerDataBaseFacade(serviceProvider.GetService<IDbCommandFacade>()));
            return serviceCollection;
        }
    }

   
}
