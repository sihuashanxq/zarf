using System.Linq.Expressions;
using Zarf.Query.Handlers.NodeTypes;
using System.Collections.Concurrent;

namespace Zarf.Query.Handlers
{
    public class DefaultQueryNodeHanlderProvider : IQueryNodeHandlerProvider
    {
        private ConcurrentDictionary<ExpressionType, IQueryNodeHandler> _handlers;

        public IQueryNodeHandler GetHandler(IQueryContext queryContext, IQueryCompiler queryCompiler, Expression node)
        {
            if (_handlers == null || _handlers[ExpressionType.Call].QueryContext != queryContext)
            {
                Initialize(queryContext, queryCompiler);
            }

            if (_handlers.TryGetValue(node.NodeType, out IQueryNodeHandler translator))
            {
                return translator;
            }

            return null;
        }

        protected virtual void Initialize(IQueryContext queryContext, IQueryCompiler queryCompiler)
        {
            _handlers = new ConcurrentDictionary<ExpressionType, IQueryNodeHandler>
            {
                [ExpressionType.Call] = new MethodCallNodeHandler(queryContext, queryCompiler),
                [ExpressionType.MemberAccess] = new MemberNodeHandler(queryContext, queryCompiler),
                [ExpressionType.Constant] = new ConstantNodeHandler(queryContext, queryCompiler),
                [ExpressionType.TypeIs] = new TypeBinaryNodeHandler(queryContext, queryCompiler),
                [ExpressionType.Parameter] = new ParameterNodeHandler(queryContext, queryCompiler),
                [ExpressionType.New] = new NewNodeHandler(queryContext, queryCompiler),
                [ExpressionType.MemberInit] = new MemberInitNodeHandler(queryContext, queryCompiler)
            };
        }

        protected void RegisterHandler(ExpressionType expressionType, IQueryNodeHandler translaor)
        {
            if (_handlers == null)
            {
                Initialize(translaor.QueryContext, translaor.QueryCompiler);
            }

            _handlers.AddOrUpdate(expressionType, translaor, (k, v) => translaor);
        }
    }
}
