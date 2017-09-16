using Zarf.Mapping.Bindings.BinderProviders;
using System.Collections.Generic;

namespace Zarf.Mapping.Bindings
{
    public class EntityBinderProvider : IEntityBinderProvider
    {
        public static readonly IEntityBinderProvider Default = new EntityBinderProvider();

        protected static readonly List<IEntityBinderProvider> Providers = new List<IEntityBinderProvider>
        {
            new EntityMemberBinderProvider(),
            new EntityTypeBinderProvider()
        };

        private EntityBinderProvider()
        {

        }

        public IEntityBinder GetBinder(IBindingContext bindingContext)
        {
            foreach (var binderProvider in Providers)
            {
                var binder = binderProvider.GetBinder(bindingContext);
                if (binder != null)
                {
                    return binder;
                }
            }
            return null;
        }
    }
}
