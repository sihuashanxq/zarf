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
                VisitQuery(node.As<QueryExpression>());
                return node;
            }

            if (node.NodeType == ExpressionType.Extension)
            {
                return node;
            }

            return base.Visit(node);
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                if (newExpression.Arguments[i].Is<QueryExpression>())
                {
                    VisitQuery(newExpression.Arguments[i].As<QueryExpression>());
                    continue;
                }

                var modelExpression = Query.QueryModel.GetModelExpression(newExpression.Members[i].DeclaringType);
                if (modelExpression == null) continue;
                var query = Context.MemberBindingMapper.GetMapedExpression(Expression.MakeMemberAccess(modelExpression, newExpression.Members[i]));

                if (query != null && query.Is<QueryExpression>())
                {
                    VisitQuery(query.As<QueryExpression>());
                }
            }

            return newExpression;
        }

        protected virtual Expression VisitQuery(QueryExpression query)
        {
            if (Query.ConstainsQuery(query))
            {
                return query;
            }

            Expression joinOn = null;

            foreach (var item in query.Projections)
            {
                var mapped = query.ExpressionMapper.GetMappedProjection(item);
                if (!mapped.Is<AggregateExpression>())
                {
                    continue;
                }

                var refrencedColumns = new SubQueryAggregateColumnRefrenceExpressionVisitor
                    (query, mapped.As<AggregateExpression>()).RefrencedColumns;

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
            Query.QueryModel = new QueryEntityModel(SubQueryModelExpressionVisitor.Visit(Query.QueryModel.Model), Query.QueryModel.ModelElementType, Query.QueryModel);
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
