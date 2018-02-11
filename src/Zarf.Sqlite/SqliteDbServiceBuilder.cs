using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using Zarf.Core;
using Zarf.Generators.Functions;
using Zarf.Generators.Functions.Registrars;

namespace Zarf.Sqlite
{
    public class SqliteDbServiceBuilder : DbServiceBuilder
    {
        private static List<ISQLFunctionHandler> _handlers;

        static SqliteDbServiceBuilder()
        {
            _handlers = new List<ISQLFunctionHandler>();

            foreach (var item in Assembly
                    .GetExecutingAssembly()
                    .GetExportedTypes()
                    .Where(t => typeof(ISQLFunctionHandler).IsAssignableFrom(t) && !t.IsAbstract))
            {
                _handlers.Add(Activator.CreateInstance(item) as ISQLFunctionHandler);
            }
        }

        public override IDbService BuildService(string connectionString, IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IDbEntityConnectionFacotry>(
                p => new SqliteDbEntityConnectionFacotry(connectionString));

            return RegisterFuntionHandlers(new DbService(connectionString, serviceCollection.BuildServiceProvider()));
        }

        protected DbService RegisterFuntionHandlers(DbService service)
        {
            var registrar = service.ServiceProvder.GetService<ISQLFunctionHandlerRegistrar>();

            foreach (var item in _handlers)
            {
                registrar.Register(item);
            }

            return service;
        }

        public override IDbService BuildService(string connectionString)
        {
            return BuildService(connectionString, new ServiceCollection().UseSqliteCore());
        }
    }
}
