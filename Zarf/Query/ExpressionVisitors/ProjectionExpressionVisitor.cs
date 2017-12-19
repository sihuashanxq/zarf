using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    /// <summary>
    /// 根据子查询生成Column
    /// </summary>
    public class ProjectionExpressionVisitor : ExpressionVisitorBase
    {
        public QueryExpression Query { get; }

        public ProjectionOwnerMapper Owner { get; }

        public ILambdaParameterMapper ParameterMapper { get; }

        public ProjectionExpressionVisitor(QueryExpression query, ProjectionOwnerMapper owner, ILambdaParameterMapper parameterMapper)
        {
            Query = query;
            Owner = owner;
            ParameterMapper = parameterMapper;
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
                    Visit(binding.Expression.As<ParameterExpression>());
                    continue;
                }

                Query.AddProjection(binding.Expression);
                Owner.AddProjection(binding.Expression, Query);
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
                    Visit(newExpression.Arguments[i].As<ParameterExpression>());
                    continue;
                }

                Query.AddProjection(newExpression.Arguments[i]);
                Owner.AddProjection(newExpression.Arguments[i], Query);
            }

            return newExpression;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            var map = ParameterMapper.GetMappedExpression(parameter);
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
                Owner.AddProjection(item, Query);
            }

            return parameter;
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            Visit(lambda.Body);
            return lambda;
        }
    }
}
