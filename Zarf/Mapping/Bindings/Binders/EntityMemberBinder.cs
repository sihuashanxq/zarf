using System;
using System.Linq.Expressions;
using Zarf.Extensions;
using System.Linq;

namespace Zarf.Mapping.Bindings
{
    public class EntityMemberBinder : IEntityBinder
    {
        public Expression Bind(IBindingContext bindingContext)
        {
            var memberInfoType = bindingContext.Member.GetMemberInfoType();
            var memberAccess = Expression.MakeMemberAccess(bindingContext.EntityObject, bindingContext.Member);

            if (!ReflectionUtil.SimpleTypes.Contains(memberInfoType))
            {
                var typeBindingContext = new BindingContext(memberInfoType, bindingContext.EntityObject);
                var binder = EntityBinderProvider.Default.GetBinder(typeBindingContext);

                if (binder != null)
                {
                    var binding = binder.Bind(typeBindingContext);
                    return Expression.Assign(memberAccess, binding);
                }
            }
            else
            {
                var memAccess = Expression.MakeMemberAccess(bindingContext.EntityObject, bindingContext.Member);
                return Expression.Assign(memAccess, null);
            }

            return null;
        }
    }
}
