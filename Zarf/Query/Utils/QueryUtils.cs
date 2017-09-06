using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public class QueryUtils
    {
        public static int FindProjectionOrdinal(QueryExpression rootQuery, Expression refProjection)
        {
            if (rootQuery.SubQuery != null && rootQuery.Projections.Count == 0)
            {
                return FindProjectionOrdinal(rootQuery.SubQuery, refProjection);
            }

            for (var index = 0; index < rootQuery.Projections.Count; index++)
            {
                if (rootQuery.Projections[index] == refProjection)
                {
                    return index;
                }

                var projiection = rootQuery.Projections[index].As<ColumnExpression>();
                if (projiection == null)
                {
                    continue;
                }

                if (projiection.Member == refProjection.As<ColumnExpression>()?.Member)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
