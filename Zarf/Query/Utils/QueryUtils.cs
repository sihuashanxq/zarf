using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query
{
    public class QueryUtils
    {
        public static Projection FindProjection(QueryExpression rootQuery, Expression projection)
        {
            if (rootQuery.SubQuery != null && rootQuery.ProjectionCollection.Count == 0)
            {
                return FindProjection(rootQuery.SubQuery, projection);
            }

            for (var index = 0; index < rootQuery.ProjectionCollection.Count; index++)
            {
                if (rootQuery.ProjectionCollection[index].Expression == projection ||
                    rootQuery.ProjectionCollection[index].Member == projection.As<ColumnExpression>()?.Member)
                {
                    return rootQuery.ProjectionCollection[index];
                }
            }

            return null;
        }
    }
}
