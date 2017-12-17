using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Zarf.Query.Expressions;
using System.Linq;
using Zarf.Extensions;

namespace Zarf.Query.ExpressionVisitors
{
    public class NewProjectionExpressionVisitor : ExpressionVisitorBase
    {
        public QueryExpression Query { get; }

        public ProjectionContainerMapper Container { get; }

        public NewProjectionExpressionVisitor(QueryExpression query, ProjectionContainerMapper container)
        {
            Query = query;
            Container = container;
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

                Query.AddProjection(binding.Expression);
                Container.AddProjection(binding.Expression, Query);
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

                Query.AddProjection(newExpression.Arguments[i]);
                Container.AddProjection(newExpression.Arguments[i], Query);
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
