using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Zarf.Query.Expressions;
using System.Linq;
using Zarf.Extensions;

namespace Zarf.Query.ExpressionVisitors
{
    /// <summary>
    /// 根据子查询生成Column
    /// </summary>
    public class NewProjectionExpressionVisitor : ExpressionVisitorBase
    {
        public QueryExpression Query { get; }

        public ProjectionOwnerMapper Owner { get; }

        public ILambdaParameterMapper ParameterMapper { get; }

        public NewProjectionExpressionVisitor(QueryExpression query, ProjectionOwnerMapper owner, ILambdaParameterMapper parameterMapper)
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
                    var map = ParameterMapper.GetMappedExpression(binding.Expression.As<ParameterExpression>());
                    if (map == null)
                    {
                        continue;
                    }

                    var query = map.As<QueryExpression>();
                    if (query == null)
                    {
                        continue;
                    }

                    foreach (var item in query.GenerateTableColumns())
                    {
                        Query.AddProjection(item);
                        Owner.AddProjection(item, Query);
                    }

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
                    var map = ParameterMapper.GetMappedExpression(newExpression.Arguments[i].As<ParameterExpression>());
                    if (map == null)
                    {
                        continue;
                    }

                    var query = map.As<QueryExpression>();
                    if (query == null)
                    {
                        continue;
                    }

                    foreach (var item in query.GenerateTableColumns())
                    {
                        Query.AddProjection(item);
                        Owner.AddProjection(item, Query);
                    }
                    continue;
                }

                Query.AddProjection(newExpression.Arguments[i]);
                Owner.AddProjection(newExpression.Arguments[i], Query);
            }

            return newExpression;
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            Visit(lambda.Body);
            return lambda;
        }
    }
}
