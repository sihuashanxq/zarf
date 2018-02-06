using Microsoft.Extensions.DependencyInjection;
using Zarf.Generators;
using Zarf.Generators.Functions;
using Zarf.Generators.Functions.Providers;
using Zarf.Generators.Functions.Registrars;
using Zarf.Query;
using Zarf.Query.Internals;
using Zarf.Update;
using Zarf.Update.Executors;

namespace Zarf.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseZarfCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEntityTracker, EntityTracker>();
            serviceCollection.AddScoped<IEntityEntryCache, EntityEntryCache>();
            serviceCollection.AddScoped<IQueryExecutor,QueryExecutor>();
            serviceCollection.AddScoped<IDbModifyExecutor,DbModifyExecutor>();
            serviceCollection.AddSingleton<ISQLFunctionHandlerManager, SQLFunctionHandlerManager>();
            serviceCollection.AddSingleton<IDbModificationCommandGroupBuilder, DbModificationCommandGroupBuilder>();

            serviceCollection.AddSingleton<ISQLFunctionHandlerProvider>(
                p => p.GetService<ISQLFunctionHandlerManager>());
            serviceCollection.AddSingleton<ISQLFunctionHandlerRegistrar>(
                p => p.GetService<ISQLFunctionHandlerManager>());

            serviceCollection.AddScoped<IQueryContextFactory, QueryContextFacotry>();

            return serviceCollection;
        }
    }
}
