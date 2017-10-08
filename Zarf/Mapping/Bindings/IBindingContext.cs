using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Zarf.Mapping.Bindings
{
    public interface IBindingContext
    {
        Type Type { get; }

        Expression Entity { get; }

        Expression BindExpression { get; }

        MemberInfo Member { get; }

        Expression Query { get; }

        IBindingContext CreateMemberBindingContext(Type type, Expression entity, MemberInfo member = null, Expression bindExpression = null);

        IEntityProjectionMappingProvider MappingProvider { get; }

        EntityCreationHandleProvider CreationHandleProvider { get; set; }
    }

    public class EntityCreationHandleProvider
    {
        internal Func<Expression, Expression, Expression> GetPredicate(MemberInfo member)
        {
            return null;
        }
    }
}
