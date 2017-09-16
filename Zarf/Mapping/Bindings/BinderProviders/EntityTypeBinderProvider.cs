using System;
using System.Collections.Generic;
using System.Text;
using Zarf.Mapping.Bindings.BinderProviders;
using Zarf.Extensions;
using System.Linq;

namespace Zarf.Mapping.Bindings
{
    public class EntityTypeBinderProvider : IEntityBinderProvider
    {
        private IEntityBinder _binder = new EntityTypeBinder();

        public IEntityBinder GetBinder(IBindingContext bindingContext)
        {
            if (bindingContext.Member != null)
            {
                return null;
            }

            return _binder;
        }
    }
}
