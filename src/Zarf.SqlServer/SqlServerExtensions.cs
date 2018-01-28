using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using Zarf.Core;
using Zarf.Generators;
using Zarf.Generators.Functions.Providers;
using Zarf.Query.Handlers;
using Zarf.SqlServer.Generators;
using Zarf.SqlServer.Query.Handlers;

namespace Zarf.SqlServer
{
    public static class SqlServerExtensions
    {
        private static ConcurrentDictionary<string, IServiceProvider> _serviceProviderCaches;

        static SqlServerExtensions()
        {
            _serviceProviderCaches = new ConcurrentDictionary<string, IServiceProvider>();
        }

        public static IDbService UseSqlServer(this IDbServiceBuilder serviceBuilder, string connectionString)
        {
            if (_serviceProviderCaches.TryGetValue(connectionString, out var serviceProviderCache))
            {
                return new DbService(connectionString, serviceProviderCache);
            }

            var dbService = new ServiceCollection().UseSqlServerCore().UseSqlServer(connectionString);

            _serviceProviderCaches.AddOrUpdate(connectionString, dbService.ServiceProvder, (k, v) => v);

            return dbService;
        }

        private static IDbService UseSqlServer(this IServiceCollection serviceCollection, string connectionString)
        {
            return new SqlServerDbServiceBuilder().BuildService(connectionString, serviceCollection);
        }

        internal static IServiceCollection UseSqlServerCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISQLGenerator>(
                p => new SqlServerGenerator(p.GetService<ISQLFunctionHandlerProvider>()));

            serviceCollection.AddScoped<IDbEntityCommandFacotry>(
                p => new SqlServerDbEntityCommandFactory(p.GetService<IDbEntityConnectionFacotry>()));

            serviceCollection.AddSingleton<IDbServiceBuilder, SqlServerDbServiceBuilder>();

            serviceCollection.AddScoped<IQueryNodeHandlerProvider, SqlServerQueryNodeHandlerProvider>();

            return serviceCollection.UseZarfCore();
        }
    }
}
