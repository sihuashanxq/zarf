using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Zarf.Mapping.Bindings
{
    public interface IBindingContext
    {
        Expression Query { get; }

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
