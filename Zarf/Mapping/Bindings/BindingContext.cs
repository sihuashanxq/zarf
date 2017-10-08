using System.Linq.Expressions;
using System;
using System.Reflection;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Type Type { get; }

        public Expression Entity { get; }

        public Expression Query { get; }

        public Expression BindExpression { get; }

        public MemberInfo Member { get; }

        public IEntityProjectionMappingProvider MappingProvider { get;  set; }

        public EntityCreationHandleProvider CreationHandleProvider { get; set; }

        public BindingContext(Type type, Expression entity, Expression query, MemberInfo member = null, Expression bindExpression = null)
        {
            Type = type;
            Entity = entity;
            Member = member;
            BindExpression = bindExpression;
            Query = query;
        }

        public IBindingContext CreateMemberBindingContext(Type type, Expression entity, MemberInfo member = null, Expression bindExpression = null)
        {
            return new BindingContext(type, entity, Query, member, bindExpression)
            {
                MappingProvider = MappingProvider,
                CreationHandleProvider = CreationHandleProvider
            };
        }
    }
}
