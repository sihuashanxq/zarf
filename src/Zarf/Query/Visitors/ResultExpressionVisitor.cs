using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Visitors
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
            if (node == null)
            {
                return node;
            }

            if (node is SelectExpression select)
            {
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
                var query = Context.BindingMaper.GetValue(member).As<SelectExpression>();

                if (query == null || Select.ContainsSelectExpression(query)) continue;

                if (!CombineAggregateSelect(query))
                {
                    Context.BindingMaper.Map(member, query);
                }
                else
                {
                    Context.BindingMaper.Map(member, Select.Projections.LastOrDefault());
                }
            }

            return newExpression;
        }

        protected virtual bool CombineAggregateSelect(SelectExpression select)
        {
            var aggregateRelationVisitor = new AggregateRelationExpressionVisitor(Context, select);

            foreach (var item in select.Projections)
            {
                if (!(item is AggregateExpression aggreate))
                {
                    continue;
                }

                Select.QueryModel = new QueryEntityModel(
                    Select,
                    SubQueryModelExpressionVisitor.Visit(Select.QueryModel.Model),
                    Select.QueryModel.ModelType, Select.QueryModel);

                //聚合列合并后,处理与Where条件中与外部查询关联的列
                aggregateRelationVisitor.Visit(select);

                //聚合列被合并到主查询中,移除对主查询的Cross Join
                foreach (var join in select.Joins.ToList())
                {
                    if (join.Select == Select || join.Select.ParentSelect == Select)
                    {
                        select.Joins.Remove(join);
                    }
                }

                var alias = new AliasExpression(Context.AliasGenerator.GetNewColumn(), select, null, item.Type);

                Select.AddProjection(alias);
                Select.Mapper.Map(aggreate, alias);

                return true;
            }

            return false;
        }
    }
}
