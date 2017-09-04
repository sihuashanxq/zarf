using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public class EntityProjectionMapping : IEntityProjectionMapping
    {
        public Expression Source { get; }

        public Expression Expression { get; }

        public int Ordinal { get; }

        public EntityProjectionMapping(Expression source, Expression node, int ordinal)
        {
            Expression = node;
            Ordinal = ordinal;
            Source = source;
        }
    }
}
