using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public class ResultExpressionVisitor : ExpressionVisitorBase
    {
        public QueryExpression Query { get; }

        public IQueryContext Context { get; }

        public QueryModelExpressionVisitor SubQueryModelExpressionVisitor { get; }

        public ResultExpressionVisitor(IQueryContext context, QueryExpression query)
        {
            Query = query;
            Context = context;
            SubQueryModelExpressionVisitor = new QueryModelExpressionVisitor(Context);
        }

        public override Expression Visit(Expression node)
        {
            if (node.Is<QueryExpression>())
            {
                return VisitQuery(node.As<QueryExpression>());
            }

            if (node.NodeType == ExpressionType.Extension)
            {
                return node;
            }

            return base.Visit(node);
        }

        protected virtual Expression VisitQuery(QueryExpression query)
        {
            if (Query.ConstainsQuery(query))
            {
                return query;
            }

            Expression joinOn = null;

            foreach (var item in query.Projections.OfType<AggregateExpression>())
            {
                var refrencedColumns = new SubQueryAggregateColumnRefrenceExpressionVisitor(query, item).RefrencedColumns;
                foreach (var column in refrencedColumns)
                {
                    ColumnExpression cloned = column.Clone();
                    cloned.Query = query;

                    if (joinOn == null)
                    {
                        joinOn = Expression.Equal(column, cloned);
                        continue;
                    }

                    joinOn = Expression.AndAlso(joinOn, Expression.Equal(column, cloned));
                }
            }

            Query.AddJoin(new JoinExpression(query, null, JoinType.Cross));
            Query.QueryModel.Model = SubQueryModelExpressionVisitor.Visit(Query.QueryModel.Model);
            Query.AddProjectionRange(query.Projections);
            Query.CombineCondtion(joinOn);

            return query;
        }
    }

    /// <summary>
    /// 子查询聚合列引用
    /// </summary>
    public class SubQueryAggregateColumnRefrenceExpressionVisitor : ExpressionVisitorBase
    {
        public QueryExpression Query { get; }

        /// <summary>
        /// 引用的外部聚合列
        /// </summary>
        public List<ColumnExpression> RefrencedColumns { get; }

        public SubQueryAggregateColumnRefrenceExpressionVisitor(QueryExpression query, AggregateExpression aggreate)
        {
            Query = query;
            RefrencedColumns = new List<ColumnExpression>();
            Visit(aggreate);
        }

        public override Expression Visit(Expression node)
        {
            if (node.Is<AliasExpression>())
            {
                return Visit(node.As<AliasExpression>().Expression);
            }

            if (node.Is<ColumnExpression>())
            {
                return VisitColumn(node.As<ColumnExpression>());
            }

            if (node.Is<AggregateExpression>())
            {
                return VisitAggreate(node.As<AggregateExpression>());
            }

            if (node.NodeType == ExpressionType.Extension)
            {
                return node;
            }

            return base.Visit(node);
        }

        protected virtual Expression VisitColumn(ColumnExpression columnExpression)
        {
            if (!Query.ConstainsQuery(columnExpression.Query))
            {
                RefrencedColumns.Add(columnExpression);
            }

            return columnExpression;
        }

        protected virtual Expression VisitAggreate(AggregateExpression aggregate)
        {
            Visit(aggregate.KeySelector);
            return aggregate;
        }
    }
}
