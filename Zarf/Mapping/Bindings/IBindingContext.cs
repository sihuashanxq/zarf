using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Zarf.Mapping.Bindings
{
    public interface IBindingContext
    {
        Type EntityType { get; }

        Expression EntityObject { get; }

        MemberInfo Member { get; }
    }
}
