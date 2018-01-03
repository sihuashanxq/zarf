using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Zarf.Mapping.Bindings
{
    public interface IBindingContext
    {
        Expression Query { get; }
    }
}
