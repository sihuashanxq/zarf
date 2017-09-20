using Zarf.Extensions;
using System.Linq.Expressions;
using System;

namespace Zarf.Mapping.Bindings.Binders
{
    public class EntityMemberBinder : IEntityBinder
    {
        public Expression Bind(IBindingContext bindingContext)
        {
            var typeInfo = bindingContext.Member.GetMemberInfoType();

            if (!typeInfo.IsPrimtiveType())
            {
                return BindComplexType(bindingContext, typeInfo);
            }

            return BindSimpleType(bindingContext);
        }

        private Expression BindComplexType(IBindingContext bindingContext, Type typeInfo)
        {
            var memberAccess = Expression.MakeMemberAccess(bindingContext.EntityObject, bindingContext.Member);
            var typeBindingContext = new BindingContext(typeInfo, bindingContext.EntityObject);

            var typeBinder = EntityBinderProvider.Default.GetBinder(typeBindingContext);
            if (typeBinder == null)
            {
                return null;
            }

            var binding = typeBinder.Bind(typeBindingContext);
            if (binding == null)
            {
                return null;
            }

            var creationHandle = bindingContext.CreationHandleProvider.GetPredicate(bindingContext.Member);
            if (creationHandle != null)
            {
                binding = creationHandle(bindingContext.EntityObject, binding);
            }

            return Expression.Assign(memberAccess, binding);
        }

        private Expression BindSimpleType(IBindingContext bindingContext)
        {
            var memberAccess = Expression.MakeMemberAccess(bindingContext.EntityObject, bindingContext.Member);
            var valueGetter = MemberValueGetterProvider.DefaultProvider.GetValueGetter(memberAccess.Type);

            if (valueGetter == null)
            {
                throw new NotImplementedException("not supported!");
            }

            var ordinal = bindingContext.MappingProvider.GetOrdinal(bindingContext.Member);
            if (ordinal == -1)
            {
                return null;
            }

            return Expression.Assign(memberAccess, Expression.Call(null, valueGetter, null, Expression.Constant(ordinal)));
        }
    }
}
