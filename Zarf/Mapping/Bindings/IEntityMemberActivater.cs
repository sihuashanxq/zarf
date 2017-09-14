using System.Linq.Expressions;
using System;

namespace Zarf.Mapping.Activators
{
    public interface IEntityBinder
    {
        void Bind(Type type, Expression bindExpression);
    }

    public interface IEntityMemberBinder<in TMember>
    {
        void Bind(TMember member, Expression bindExpression);
    }
}
