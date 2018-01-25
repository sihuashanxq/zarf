using Microsoft.Extensions.DependencyInjection;
using Zarf.Core;
using Zarf.Generators;
using Zarf.Query.Handlers;
using Zarf.Sqlite.Generators;
using Zarf.Sqlite.Query.Handlers;
using Zarf.Sqlite.Query.Handlers.Providers;

namespace Zarf.Sqlite
{
    public static class SqliteExtensions
    {
        public static IDbService UseSqlite(this IServiceCollection serviceCollection, string connectionString)
        {
            return new SqliteDbServiceBuilder().BuildService(connectionString, serviceCollection);
        }

        public static IDbService UseSqlite(this IDbServiceBuilder serviceBuilder, string connectionString)
        {
            return new ServiceCollection().AddSqlite().UseSqlite(connectionString);
        }

        internal static IServiceCollection AddSqlite(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISQLGenerator, SqliteGenerator>();

            serviceCollection.AddScoped<IDbEntityCommandFacotry>(
                p => new SqliteDbEntityCommandFactory(p.GetService<IDbEntityConnectionFacotry>()));

            serviceCollection.AddScoped<IDbServiceBuilder, SqliteDbServiceBuilder>();

            serviceCollection.AddScoped<IQueryNodeHandlerProvider, SqliteNodeTypeTranslatorProvider>();
    
            return serviceCollection.AddZarf();
        }
    }
}
