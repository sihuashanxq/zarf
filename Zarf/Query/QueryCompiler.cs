using System.Linq.Expressions;
using Zarf.Query.ExpressionTranslators;

namespace Zarf.Query.ExpressionVisitors
{
    public class QueryCompiler : ExpressionVisitorBase, IQueryCompiler
    {
        protected IQueryContext Context { get; }

        protected ITransaltorProvider TranslatorProvider { get; }

        public QueryCompiler(IQueryContext context)
        {
            Context = context;
            TranslatorProvider = new NodeTypeTranslatorProvider(context, this);
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

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            var queryTranslator = TranslatorProvider.GetTranslator(node);
            if (queryTranslator != null)
            {
                return queryTranslator.Translate(node);
            }

            return base.Visit(node);
        }

        public Expression Compile(Expression query)
        {
            if (query.NodeType == ExpressionType.Extension)
            {
                return query;
            }

            return Visit(query);
        }
    }
}
