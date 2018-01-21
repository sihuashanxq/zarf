using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public class ResultExpressionVisitor : ExpressionVisitorBase
    {
        public SelectExpression Select { get; }

        public IQueryContext Context { get; }

        public QueryModelExpressionVisitor SubQueryModelExpressionVisitor { get; }

        public ResultExpressionVisitor(IQueryContext context, SelectExpression select)
        {
            Select = select;
            Context = context;
            SubQueryModelExpressionVisitor = new QueryModelExpressionVisitor(Context);
        }

        public override Expression Visit(Expression node)
        {
            if (node.Is<SelectExpression>())
            {
                var select = node.As<SelectExpression>();
                if (!Select.ContainsSelectExpression(select))
                {
                    CombineAggregateSelect(select);
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
                var modelExpression = Select.QueryModel.GetModelExpression(newExpression.Members[i]);
                if (modelExpression == null) continue;

                var member = Expression.MakeMemberAccess(modelExpression, newExpression.Members[i]);
                var query = Context.MemberBindingMapper.GetMapedExpression(member).As<SelectExpression>();

                if (query == null || Select.ContainsSelectExpression(query)) continue;

                if (!CombineAggregateSelect(query))
                {
                    Context.MemberBindingMapper.Map(member, query);
                }
                else
                {
                    Context.MemberBindingMapper.Map(member, Select.Projections.LastOrDefault());
                }
            }

            return newExpression;
        }

        protected virtual bool CombineAggregateSelect(SelectExpression query)
        {
            foreach (var item in query.Projections)
            {
                if (!(item is AggregateExpression aggreate))
                {
                    continue;
                }

                Select.QueryModel = new QueryEntityModel(
                    Select,
                    SubQueryModelExpressionVisitor.Visit(Select.QueryModel.Model),
                    Select.QueryModel.ModelType, Select.QueryModel);

                //聚合列被合并到主查询中,移除对主查询的Cross Join
                foreach (var join in query.Joins.ToList())
                {
                    if (join.Select == Select || join.Select.SourceSelect == Select)
                    {
                        query.Joins.Remove(join);
                    }
                }

                var alias = new AliasExpression(Context.Alias.GetNewColumn(), query, null, item.Type);

                Select.AddProjection(alias);
                Select.ExpressionMapper.Map(aggreate, alias);

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
        public SelectExpression Select { get; }

        /// <summary>
        /// 引用的外部聚合列
        /// </summary>
        public List<ColumnExpression> RefrencedColumns { get; }

        public SubQueryAggregateColumnRefrenceExpressionVisitor(SelectExpression select, AggregateExpression aggreate)
        {
            Select = select;
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
            if (!Select.ContainsSelectExpression(columnExpression.Select))
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
