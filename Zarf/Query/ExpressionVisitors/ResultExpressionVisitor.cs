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
                var query = node.As<QueryExpression>();
                if (!Query.ConstainsQuery(query))
                {
                    CombineAggregateQuery(query);
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
                var modelExpression = Query.QueryModel.GetModelExpression(newExpression.Members[i]);
                if (modelExpression == null) continue;

                var member = Expression.MakeMemberAccess(modelExpression, newExpression.Members[i]);
                var query = Context.MemberBindingMapper.GetMapedExpression(member).As<QueryExpression>();

                if (query == null || Query.ConstainsQuery(query)) continue;

                if (!CombineAggregateQuery(query))
                {
                    Context.MemberBindingMapper.Map(member, query);
                }
                else
                {
                    Context.MemberBindingMapper.Map(member, Query.Projections.LastOrDefault());
                }
            }

            return newExpression;
        }

        protected virtual bool CombineAggregateQuery(QueryExpression query)
        {
            foreach (var item in query.Projections)
            {
                if (!(item is AggregateExpression aggreate))
                {
                    continue;
                }

                Query.QueryModel = new QueryEntityModel(
                    Query,
                    SubQueryModelExpressionVisitor.Visit(Query.QueryModel.Model),
                    Query.QueryModel.ModelType, Query.QueryModel);

                //聚合列被合并到主查询中,移除对主查询的Cross Join
                foreach (var join in query.Joins.ToList())
                {
                    if (join.Query == Query || join.Query.SourceQuery == Query)
                    {
                        query.Joins.Remove(join);
                    }
                }

                var alias = new AliasExpression(Context.Alias.GetNewColumn(), query, null, item.Type);

                Query.AddProjection(alias);
                Query.ExpressionMapper.Map(aggreate, alias);

                return true;
            }

            return false;
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
