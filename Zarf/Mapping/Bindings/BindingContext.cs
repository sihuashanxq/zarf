using System.Linq.Expressions;
using Zarf.Query.Expressions;
using Zarf.Extensions;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public QueryExpression RootQuery { get; }

        public BindingContext(QueryExpression rootQuery)
        {
            RootQuery = rootQuery;
        }

        public int GetExpressionOrdinal(Expression node)
        {
            return GetExpressionOridinal(node, RootQuery);
        }

        internal static int GetExpressionOridinal(Expression node, QueryExpression queryExpression)
        {
            if (queryExpression == null || node == null)
            {
                return -1;
            }

            if (queryExpression.Projections.Count == 0 && queryExpression.SubQuery != null)
            {
                return GetExpressionOridinal(node, queryExpression.SubQuery);
            }

            var oridinal = -1;

            foreach (var item in queryExpression.Projections)
            {
                if (item == node)
                {
                    return oridinal;
                }

                if (node.Is<ColumnExpression>() && item.Is<ColumnExpression>())
                {
                    var column = item.As<ColumnExpression>();
                    var other = node.As<ColumnExpression>();

                    if (column?.Member == other?.Member)
                    {
                        return oridinal;
                    }
                }

                oridinal++;
            }

            return -1;
        }
    }
}
