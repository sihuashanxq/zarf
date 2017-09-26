using System.Linq.Expressions;
using Zarf.Query.Expressions;
using Zarf.Extensions;
using System;
using System.Reflection;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Type Type { get; }

        public Expression Entity { get; }

        public MemberInfo Member { get; }

        public Expression BindExpression { get; }

        public IEntityProjectionMappingProvider MappingProvider { get; private set; }

        public EntityCreationHandleProvider CreationHandleProvider { get; set; }

        public BindingContext(Type type, Expression entity, MemberInfo member = null, Expression bindExpression = null)
        {
            Type = type;
            Entity = entity;
            Member = member;
            BindExpression = bindExpression;
        }

        public IBindingContext CreateContext(Type type, Expression entity, MemberInfo member = null, Expression bindExpression = null)
        {
            return new BindingContext(type, entity, member, bindExpression)
            {
                MappingProvider = MappingProvider,
                CreationHandleProvider = CreationHandleProvider
            };
        }
    }
}
