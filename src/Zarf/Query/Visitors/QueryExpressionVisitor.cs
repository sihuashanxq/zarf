using System.Linq.Expressions;
using Zarf.Query.Handlers;

namespace Zarf.Query.Visitors
{
    public class QueryExpressionVisitor : ExpressionVisitorBase, IQueryCompiler
    {
        protected IQueryContext QueryContext { get; }

        protected IQueryNodeHandlerProvider Provider { get; }

        public QueryExpressionVisitor(IQueryContext queryContext)
        {
            QueryContext = queryContext;
            Provider = (IQueryNodeHandlerProvider)QueryContext.DbContext.ServiceProvider.GetService(typeof(IQueryNodeHandlerProvider));
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            return Provider.GetHandler(QueryContext,this,node)?.HandleNode(node) ?? base.Visit(node);
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
