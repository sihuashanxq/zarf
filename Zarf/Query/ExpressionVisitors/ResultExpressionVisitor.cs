using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.NodeTypes;

namespace Zarf.Query.ExpressionVisitors
{
    public class ResultExpressionVisitor : ExpressionVisitorBase
    {
        public QueryExpression Query { get; }

        public ResultExpressionVisitor(QueryExpression query)
        {
            Query = query;
        }

        public override Expression Visit(Expression node)
        {
            if (node.Is<QueryExpression>())
            {
                var query = node.As<QueryExpression>();
                if (query == Query)
                {
                    return node;
                }

                //Map
                foreach (var item in query.Projections)
                {
                    if (item.Is<AliasExpression>())
                    {
                        var alias = item.As<AliasExpression>();
                        Query.AddProjection(new ColumnExpression(query, new Column(alias.Alias), alias.Type));
                    }

                    if (item.Is<ColumnExpression>())
                    {
                        //query==Query, join Equal
                        var col = item.As<ColumnExpression>();
                        var colName = col.Column?.Name ?? col.Alias;
                        Query.AddProjection(new ColumnExpression(query, new Column(colName), col.Type));
                    }

                    if (item.Is<AggregateExpression>())
                    {
                        var agg = item.As<AggregateExpression>();
                        Query.AddProjection(new ColumnExpression(query, new Column(agg.Alias), agg.Type));
                    }
                }

                Query.AddJoin(new JoinExpression(query, null, JoinType.Cross));
                return node;
            }

            return base.Visit(node);
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            var lambdaBody = Visit(lambda.Body);
            if (lambdaBody != lambda.Body)
            {
                return Expression.Lambda(lambdaBody, lambda.Parameters);
            }

            return lambda;
        }
    }
}
