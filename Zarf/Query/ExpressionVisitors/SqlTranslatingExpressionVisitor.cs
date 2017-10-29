using System.Linq.Expressions;
using Zarf.Query.ExpressionTranslators;

namespace Zarf.Query.ExpressionVisitors
{
    public class SqlTranslatingExpressionVisitor : ExpressionVisitorBase
    {
        protected IQueryContext Context { get; }

        protected ITransaltorProvider TranslatorProvider { get; }

        public SqlTranslatingExpressionVisitor(IQueryContext context, ITransaltorProvider transaltorProvider)
        {
            Context = context;
            TranslatorProvider = transaltorProvider;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            var translator = TranslatorProvider.GetTranslator(node);
            if (translator != null)
            {
                return translator.Translate(Context, node, this);
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
