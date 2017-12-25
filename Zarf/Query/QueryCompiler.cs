using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
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

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            return TranslatorProvider.GetTranslator(node)?.Translate(node) ?? base.Visit(node);
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
