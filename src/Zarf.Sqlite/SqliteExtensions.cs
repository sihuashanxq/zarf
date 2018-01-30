using Microsoft.Extensions.DependencyInjection;
using System.Data.SQLite;
using System.Reflection;
using Zarf.Core;
using Zarf.Generators;
using Zarf.Generators.Functions.Providers;
using Zarf.Query.Handlers;
using Zarf.Sqlite.Generators;
using Zarf.Sqlite.Query.Handlers.Providers;
using System.Collections.Concurrent;
using System;
using Zarf.Update;
using Zarf.Sqlite.Update;

namespace Zarf.Sqlite
{
    public static class SqliteExtensions
    {
        private static ConcurrentDictionary<string, IServiceProvider> _serviceProviderCaches;

        static SqliteExtensions()
        {
            //注册自定义函数
            foreach (var item in Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => typeof(SQLiteFunction).IsAssignableFrom(t)))
            {
                SQLiteFunction.RegisterFunction(item);
            }

            _serviceProviderCaches = new ConcurrentDictionary<string, IServiceProvider>();
        }

        public static IDbService UseSqlite(this IDbServiceBuilder serviceBuilder, string connectionString)
        {
            if (_serviceProviderCaches.TryGetValue(connectionString, out var serviceProviderCache))
            {
                return new DbService(connectionString, serviceProviderCache);
            }

            var dbService = new ServiceCollection().UseSqliteCore().UseSqlite(connectionString);

            _serviceProviderCaches.AddOrUpdate(connectionString, dbService.ServiceProvder, (k, v) => v);

            return dbService;
        }

        private static IDbService UseSqlite(this IServiceCollection serviceCollection, string connectionString)
        {
            return new SqliteDbServiceBuilder().BuildService(connectionString, serviceCollection);
        }

        internal static IServiceCollection UseSqliteCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISQLGenerator>(
               p => new SqliteGenerator(p.GetService<ISQLFunctionHandlerProvider>()));

            serviceCollection.AddScoped<IDbEntityCommandFacotry>(
                p => new SqliteDbEntityCommandFactory(p.GetService<IDbEntityConnectionFacotry>()));

            serviceCollection.AddScoped<IDbServiceBuilder, SqliteDbServiceBuilder>();

            serviceCollection.AddScoped<IQueryNodeHandlerProvider, SqliteNodeTypeTranslatorProvider>();

            serviceCollection.AddSingleton<IDbModificationCommandGroupBuilder, SqliteDbModifycationCommandGroupBuilder>();

            return serviceCollection.UseZarfCore();
        }
    }
}
