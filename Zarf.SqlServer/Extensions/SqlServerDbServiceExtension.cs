using Microsoft.Extensions.DependencyInjection;
using Zarf.Builders;
using Zarf.SqlServer.Builders;

namespace Zarf.SqlServer.Extensions
{
    public static class SqlServerDbServiceExtension
    {
        public static IServiceCollection AddZarfSqlServer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISqlTextBuilder, SqlServerTextBuilder>();
            return serviceCollection;
        }
    }
}
