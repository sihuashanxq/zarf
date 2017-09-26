using Zarf.Mapping.Bindings.BinderProviders;
using Zarf.Mapping.Bindings.Binders;

namespace Zarf.Mapping.Bindings
{
    public class EntityMemberBinderProvider : IEntityBinderProvider
    {
        public IEntityBinder GetBinder(IBindingContext bindingContext)
        {
            if (bindingContext.Member == null)
            {
                return null;
            }

            return new EntityMemberBinder();
        }
    }
}
