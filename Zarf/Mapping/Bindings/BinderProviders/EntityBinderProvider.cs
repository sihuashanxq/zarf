using Zarf.Mapping.Bindings.BinderProviders;
using System.Collections.Generic;

namespace Zarf.Mapping.Bindings
{
    public static class EntityBinderProviders
    {
        public static readonly List<IEntityBinderProvider> BinderProviders = new List<IEntityBinderProvider>()
        {
            new EntityMemberBinderProvider(),
            new EntityTypeBinderProvider()
        };

        public static IEntityBinder GetBinder(IBindingContext bindingContext)
        {
            foreach (var provider in BinderProviders)
            {
                var binder = provider.GetBinder(bindingContext);
                if (binder != null)
                {
                    return binder;
                }
            }

            return null;
        }
    }
}
