using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public class QueryUtils
    {
        public static int FindExpressionIndex(QueryExpression rootQuery, Expression node)
        {
            if (rootQuery.SubQuery != null && rootQuery.Projections.Count == 0)
            {
                return FindExpressionIndex(rootQuery.SubQuery, node);
            }

            for (var i = 0; i < rootQuery.Projections.Count; i++)
            {
                if (rootQuery.Projections[i] == node)
                {
                    return i;
                }

                var projiection = rootQuery.Projections[i].As<ColumnExpression>();
                if (projiection == null)
                {
                    continue;
                }

                if (projiection.Member == node.As<ColumnExpression>()?.Member)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
