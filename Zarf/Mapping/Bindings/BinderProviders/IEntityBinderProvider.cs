using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Mapping.Bindings.BinderProviders
{
    public interface IEntityBinderProvider
    {
        IEntityBinder GetBinder(IBindingContext bindingContext);
    }
}
