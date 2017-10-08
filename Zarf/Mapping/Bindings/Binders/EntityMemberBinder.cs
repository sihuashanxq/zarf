using System;
using System.Data;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Mapping.Bindings.Binders
{
    public class EntityMemberBinder : IEntityBinder
    {
        public static readonly ParameterExpression DataReader = Expression.Parameter(typeof(IDataReader));

        public Expression Bind(IBindingContext bindingContext)
        {
            var typeInfo = bindingContext.Member.GetMemberTypeInfo();
            if (!typeInfo.IsPrimtiveType())
            {
                return BindComplexType(bindingContext, typeInfo);
            }

            return BindSimpleType(bindingContext);
        }

        private Expression BindComplexType(IBindingContext bindingContext, Type typeInfo)
        {
            var memberAccess = Expression.MakeMemberAccess(bindingContext.Entity, bindingContext.Member);
            var typeBindingContext = bindingContext.CreateMemberBindingContext(typeInfo, bindingContext.Entity);
            var binder = EntityBinderProviders.GetBinder(typeBindingContext);
            var binding = binder?.Bind(typeBindingContext);

            if (binding == null)
            {
                return null;
            }

            var creationHandle = bindingContext.CreationHandleProvider.GetPredicate(bindingContext.Member);
            if (creationHandle != null)
            {
                binding = creationHandle(bindingContext.Entity, binding);
            }

            return Expression.Assign(memberAccess, binding);
        }

        private Expression BindSimpleType(IBindingContext bindingContext)
        {
            var memberAccess = Expression.MakeMemberAccess(bindingContext.Entity, bindingContext.Member);
            var valueGetter = MemberValueGetterProvider.DefaultProvider.GetValueGetter(memberAccess.Type);

            if (valueGetter == null)
            {
                throw new NotImplementedException("not supported!");
            }

            var mapOrdinal = -1;
            if (bindingContext.BindExpression != null)
            {
                mapOrdinal = bindingContext.MappingProvider.GetOrdinal(bindingContext.Query, bindingContext.BindExpression);
            }
            else
            {
                mapOrdinal = bindingContext.MappingProvider.GetOrdinal(bindingContext.Query, bindingContext.Member);
            }

            if (mapOrdinal == -1)
            {
                return null;
            }

            return Expression.Assign(memberAccess, Expression.Call(null, valueGetter, DataReader, Expression.Constant(mapOrdinal)));
        }
    }
}
