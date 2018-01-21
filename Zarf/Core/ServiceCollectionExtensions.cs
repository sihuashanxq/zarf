using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Zarf.Generators;
using Zarf.Query;
using Zarf.Query.Internals;
using Zarf.Update;
using Zarf.Update.Executors;

namespace Zarf.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddZarf(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IEntityTracker, EntityTracker>();
            serviceCollection.AddScoped<IEntityEntryCache, EntityEntryCache>();

            serviceCollection.AddScoped<IQueryExecutor>(
                (p) => new QueryExecutor(
                    p.GetService<ISQLGenerator>(),
                    p.GetService<IDbEntityCommandFacotry>(),
                    p.GetService<IDbEntityConnectionFacotry>()));

            serviceCollection.AddScoped<IDbModifyExecutor>(
                (p) => new DbModifyExecutor(
                    p.GetService<IDbEntityCommandFacotry>(),
                    p.GetService<ISQLGenerator>(),
                    p.GetService<IEntityTracker>()
                ));

            serviceCollection.AddSingleton<IQueryContextFactory, QueryContextFacotry>();

            return serviceCollection;
        }
    }
}
