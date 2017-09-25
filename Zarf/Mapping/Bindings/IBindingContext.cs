using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Zarf.Mapping.Bindings
{
    public interface IBindingContext
    {
        Type Type { get; }

        Expression EntityObject { get; }

        Expression BindExpression { get; }

        MemberInfo Member { get; }

        IEntityProjectionMappingProvider MappingProvider { get; }

        EntityCreationHandleProvider CreationHandleProvider { get; set; }
    }

    public class EntityCreationHandleProvider
    {
        internal object GetPredicate(MemberInfo member)
        {
            throw new NotImplementedException();
        }
    }
}
