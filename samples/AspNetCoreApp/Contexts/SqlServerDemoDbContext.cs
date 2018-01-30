using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zarf;
using Zarf.Core;
using Zarf.SqlServer;

namespace AspNetCoreApp.Contexts
{
    public class SqlServerDemoDbContext : DbContext
    {
        public SqlServerDemoDbContext(Func<IDbServiceBuilder, IDbService> serviceBuilder) 
            : base(serviceBuilder)
        {

        }
    }
}
