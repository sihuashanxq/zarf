using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    /// <summary>
    /// 根据子查询生成Column
    /// </summary>
    public class ProjectionExpressionVisitor : QueryCompiler
    {
        public QueryExpression Query { get; }

        public ProjectionExpressionVisitor(QueryExpression query, IQueryContext queryContext)
            : base(queryContext)
        {
            Query = query;
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInit)
        {
            Visit(memberInit.NewExpression);

            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                if (binding.Expression.Is<QueryExpression>())
                {
                    continue;
                }

                if (binding.Expression.NodeType == ExpressionType.Parameter)
                {
                    VisitParameter(binding.Expression.As<ParameterExpression>());
                    continue;
                }

                Query.AddProjection(binding.Expression);
                Context.ProjectionOwner.AddProjection(binding.Expression, Query);
            }

            return memberInit;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                if (newExpression.Arguments[i].Is<QueryExpression>())
                {
                    continue;
                }

                if (newExpression.Arguments[i].NodeType == ExpressionType.Parameter)
                {
                    VisitParameter(newExpression.Arguments[i].As<ParameterExpression>());
                    continue;
                }

                Query.AddProjection(newExpression.Arguments[i]);
                Context.ProjectionOwner.AddProjection(newExpression.Arguments[i], Query);
            }

            return newExpression;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            var map = Context.ParameterQueryMapper.GetMappedExpression(parameter);
            if (map == null)
            {
                return parameter;
            }

            var query = map.As<QueryExpression>();
            if (query == null)
            {
                return parameter;
            }

            foreach (var item in query.GenQueryProjections())
            {
                Query.AddProjection(item);
                Context.ProjectionOwner.AddProjection(item, Query);
            }

            return parameter;
        }

        public override Expression Visit(Expression node)
        {
            if (node.NodeType == ExpressionType.New)
            {
                return VisitNew(base.Visit(node).As<NewExpression>());
            }

            if (node.NodeType == ExpressionType.MemberInit)
            {
                return VisitMemberInit(base.Visit(node).As<MemberInitExpression>());
            }

            return base.Visit(node);
        }
    }
}
