using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query
{
    public interface IQueryContextFactory
    {
        IQueryContext CreateContext();
    }
}
