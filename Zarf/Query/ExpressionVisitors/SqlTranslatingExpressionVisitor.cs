using System.Linq.Expressions;
using Zarf.Query.ExpressionTranslators;

namespace Zarf.Query.ExpressionVisitors
{
    public class SqlTranslatingExpressionVisitor : ZarfExpressionVisitor
    {
        private QueryContext _queryContext;

        private ITransaltorProvider _transaltorProvider;

        public SqlTranslatingExpressionVisitor(QueryContext queryContext, ITransaltorProvider provider)
        {
            _queryContext = queryContext;
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
                return translator.Translate(_queryContext, node, this);
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
