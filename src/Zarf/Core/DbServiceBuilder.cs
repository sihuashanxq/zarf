using Microsoft.Extensions.DependencyInjection;
using System;
using Zarf.Generators;
using Zarf.Query;
using Zarf.Query.Internals;
using Zarf.Update;

namespace Zarf.Core
{
    public class DbServiceBuilder : IDbServiceBuilder
    {
        public virtual IDbService BuildService(string connectionString, IServiceCollection serviceCollection)
        {
            throw new NotImplementedException();
        }

        public virtual IDbService BuildService(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
