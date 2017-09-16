using System.Linq.Expressions;
using System;

namespace Zarf.Mapping.Bindings
{
    public interface IEntityBinder
    {
        Expression Bind(IBindingContext bindingContext);
    }
}
