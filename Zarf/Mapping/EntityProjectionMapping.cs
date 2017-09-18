using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping
{
    public class EntityProjectionMapping : IEntityProjectionMapping
    {
        public Expression Source { get; }

        public Expression Expression { get; }

        public int Ordinal { get; }

        public MemberInfo Member { get; }

        public EntityProjectionMapping(Expression source, Expression node, MemberInfo member, int ordinal)
        {
            Expression = node;
            Ordinal = ordinal;
            Source = source;
            Member = member;
        }
    }
}
