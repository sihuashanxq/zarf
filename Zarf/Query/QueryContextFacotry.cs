using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query
{
    public class QueryContextFacotry : IQueryContextFactory
    {
        public IQueryContext CreateContext()
        {
            return new QueryContext();
        }
    }
}
