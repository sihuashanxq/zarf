using System.Linq.Expressions;
using System;

namespace Zarf.Mapping.Activators
{
    public interface IEntityBinder
    {
        Expression Bind(Type type, Expression entityNewExpression);
    }

    public interface IEntityMemberBinder<in TMember>
    {
        Expression Bind(TMember member, Expression bindingExpression);
    }
}
