using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Zarf.Query.Expressions;
using System.Linq;

namespace Zarf.Query.ExpressionVisitors
{
    public class NewProjectionExpressionVisitor : ExpressionVisitorBase
    {
        public QueryExpression Query { get; }

        public NewProjectionExpressionVisitor(QueryExpression query)
        {
            Query = query;
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInit)
        {
            Visit(memberInit.NewExpression);

            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                Query.AddProjection(binding.Expression);
            }

            return memberInit;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                Query.AddProjection(newExpression.Arguments[i]);
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
