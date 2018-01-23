using Microsoft.Extensions.DependencyInjection;
using System;

namespace Zarf.Core
{
    public class DbService : IDbService
    {
        public string ConnectionString { get; }

        public IServiceProvider ServiceProvder { get; }

        public IDbEntityConnection EntityConnection { get; }

        public DbService(string connectionString, IServiceProvider serviceProvider)
        {
            ConnectionString = connectionString;
            ServiceProvder = serviceProvider;
            EntityConnection = serviceProvider.GetService<IDbEntityConnectionFacotry>().Create();
        }
    }
}
