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
            //var x = NewExpressionTranslator.Maped;
            //if (NewExpressionTranslator.Maped.ContainsKey(node))
            //{
            //    return Expression.Constant(1);
            //}

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

        //        if (node.Is<QueryExpression>())
        //            {
        //                var query = node.As<QueryExpression>();
        //                if (query == Root)
        //                {
        //                    return node;
        //                }

        //                if (query.Columns.Count == 0)
        //                {
        //                    foreach (var item in query.GenerateTableColumns())
        //                    {
        //                        var col = new ColumnDescriptor()
        //                        {
        //                            Member = item.As<ColumnExpression>()?.Member,
        //                            Expression = item
        //                        };

        //    query.AddColumns(new[] { col
        //});
        //                    }
        //                }

        //                query.Limit = 1;
        //                Root.AddJoin(new JoinExpression(query, null, JoinType.Cross));
        //                return node;
        //            }
        //            else if (node.NodeType == ExpressionType.Extension)
        //            {
        //                return node;
        //            }

        //            return base.Visit(node);

        //            if (node.Is<AllExpression>())
        //            {

        //            }

        //            if (node.Is<AnyExpression>())
        //            {

        //            }

    }
}
