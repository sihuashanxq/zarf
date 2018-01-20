using System;

namespace Zarf.Mapping.Bindings
{
    public interface IBinder
    {
        Delegate Bind<TEntity>(IBindingContext bindingContext);
    }
}
