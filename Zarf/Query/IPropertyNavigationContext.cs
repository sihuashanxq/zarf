using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query
{
    public interface IPropertyNavigationContext
    {
        IQuerySourceProvider QuerySourceProvider { get; }
    }
}
