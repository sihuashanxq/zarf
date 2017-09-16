using Zarf.Mapping.Bindings.BinderProviders;

namespace Zarf.Mapping.Bindings
{
    public class EntityMemberBinderProvider : IEntityBinderProvider
    {
        private IEntityBinder _binder = new EntityMemberBinder();

        public IEntityBinder GetBinder(IBindingContext bindingContext)
        {
            if (bindingContext.Member == null)
            {
                return null;
            }

            return _binder;
        }
    }
}
