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

        public Expression EntityObject { get; }

        public MemberInfo Member { get; }

        public BindingContext(Type entityType, Expression entityObject, MemberInfo member = null)
        {
            Type = entityType;
            EntityObject = entityObject;
            Member = member;
        }
    }
}
