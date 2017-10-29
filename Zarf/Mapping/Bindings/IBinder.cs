using System.Linq.Expressions;
using System;

namespace Zarf.Mapping.Bindings
{
    public interface IBinder
    {
        Delegate Bind(IBindingContext bindingContext);
    }
}
