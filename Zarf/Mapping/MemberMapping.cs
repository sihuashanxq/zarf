using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public class MemberMapping : IMapping
    {
        public Expression Expression { get; }

        public int Ordinal { get; }

        public Expression Source { get; }

        public MemberMapping(Expression source, Expression node, int ordinal)
        {
            Expression = node;
            Ordinal = ordinal;
            Source = source;
        }
    }
}
