using System.Linq.Expressions;
using Zarf.Query.ExpressionTranslators;

namespace Zarf.Query.ExpressionVisitors
{
    public class SqlTranslatingExpressionVisitor : ExpressionVisitorBase
    {
        private IQueryContext _context;

        private ITransaltorProvider _transaltorProvider;

        public SqlTranslatingExpressionVisitor(IQueryContext context, ITransaltorProvider provider)
        {
            _context = context;
            _transaltorProvider = provider;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            var translator = _transaltorProvider.GetTranslator(node);
            if (translator != null)
            {
                return translator.Translate(_context, node, this);
            }

            return base.Visit(node);
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            var lambdaBody = Visit(lambda.Body);
            if (lambdaBody != lambda.Body)
            {
                return Expression.Lambda(lambdaBody, lambda.Parameters);
            }

            return lambdaBody;
        }
    }
}
