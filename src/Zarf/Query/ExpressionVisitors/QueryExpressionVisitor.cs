using System.Linq.Expressions;
using Zarf.Query.ExpressionTranslators;

namespace Zarf.Query.ExpressionVisitors
{
    public class QueryExpressionVisitor : ExpressionVisitorBase, IQueryCompiler
    {
        protected IQueryContext QueryContext { get; }

        protected ITransaltorProvider Provider { get; }

        public QueryExpressionVisitor(IQueryContext queryContext)
        {
            QueryContext = queryContext;
            Provider = new NodeTypeTranslatorProvider(queryContext, this);
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            return Provider.GetTranslator(node)?.Translate(node) ?? base.Visit(node);
        }

        public virtual Expression Compile(Expression query)
        {
            if (query == null)
            {
                return query;
            }

            if (query.NodeType == ExpressionType.Extension)
            {
                return query;
            }

            return Visit(query);
        }
    }
}
