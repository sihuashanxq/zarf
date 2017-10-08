using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public class Projection
    {
        public Expression Expression { get; set; }

        public int Ordinal { get; set; } = -1;

        public MemberInfo Member { get; set; }

        public FromTableExpression Query { get; set; }
    }
}
