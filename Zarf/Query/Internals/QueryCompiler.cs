using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators;

namespace Zarf.Query.ExpressionVisitors
{
    public class QueryCompiler : ExpressionVisitorBase, IQueryCompiler
    {
        protected IQueryContext QueryContext { get; }

        protected ITransaltorProvider Provider { get; }

        public QueryCompiler(IQueryContext queryContext)
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
