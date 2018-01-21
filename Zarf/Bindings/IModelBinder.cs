using System;

namespace Zarf.Bindings
{
    public interface IModelBinder
    {
        Delegate Bind<TEntity>(IBindingContext bindingContext);
    }
}
