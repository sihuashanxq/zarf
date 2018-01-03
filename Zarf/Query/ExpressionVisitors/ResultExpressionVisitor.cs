using System.Collections.Generic;
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
                var query = node.As<QueryExpression>();
                if (!Query.ConstainsQuery(query))
                {
                    CombineQuery(query);
                }
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
                var modelExpression = Query.QueryModel.GetModelExpression(newExpression.Members[i].DeclaringType);
                if (modelExpression == null) continue;

                var member = Expression.MakeMemberAccess(modelExpression, newExpression.Members[i]);
                var query = Context.MemberBindingMapper.GetMapedExpression(member).As<QueryExpression>();

                if (query == null || Query.ConstainsQuery(query)) continue;

                if (!CombineQuery(query))
                {
                    Context.MemberBindingMapper.Map(member, query);
                }
            }

            return newExpression;
        }

        protected virtual bool CombineQuery(QueryExpression query)
        {
            var joinOn = null as Expression;
            var combined = false;

            foreach (var item in query.Projections)
            {
                var mappedProjection = query.ExpressionMapper.GetMappedProjection(item).As<AggregateExpression>();
                if (mappedProjection == null)
                {
                    continue;
                }

                var subQueryAggreateVisitor = new SubQueryAggregateColumnRefrenceExpressionVisitor(query, mappedProjection);

                foreach (var column in subQueryAggreateVisitor.RefrencedColumns)
                {
                    var col = column.Clone();

                    col.Query = query;

                    joinOn = joinOn == null
                        ? Expression.Equal(column, col)
                        : Expression.AndAlso(joinOn, Expression.Equal(column, col));
                }

                combined = true;
            }

            if (!combined)
            {
                return false;
            }

            Query.AddJoin(new JoinExpression(query, null, JoinType.Cross));
            Query.QueryModel = new QueryEntityModel(Query, SubQueryModelExpressionVisitor.Visit(Query.QueryModel.Model), Query.QueryModel.ModeType, Query.QueryModel);
            Query.AddProjectionRange(query.Projections);
            Query.CombineCondtion(joinOn);

            return true;
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
