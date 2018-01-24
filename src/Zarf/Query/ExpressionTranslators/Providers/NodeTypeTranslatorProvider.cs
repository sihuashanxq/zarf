using System.Linq.Expressions;
using Zarf.Query.ExpressionTranslators.NodeTypes;
using System.Collections.Concurrent;

namespace Zarf.Query.ExpressionTranslators
{
    public class NodeTypeTranslatorProvider : ITransaltorProvider
    {
        private ConcurrentDictionary<ExpressionType, ITranslator> _nodeTypeTranslators;

        public ITranslator GetTranslator(IQueryContext queryContext, IQueryCompiler queryCompiler, Expression node)
        {
            if (_nodeTypeTranslators == null)
            {
                Initialize(queryContext, queryCompiler);
            }

            if (_nodeTypeTranslators.TryGetValue(node.NodeType, out ITranslator translator))
            {
                return translator;
            }

            return null;
        }

        protected virtual void Initialize(IQueryContext queryContext, IQueryCompiler queryCompiler)
        {
            _nodeTypeTranslators = new ConcurrentDictionary<ExpressionType, ITranslator>
            {
                [ExpressionType.Call] = new MethodCallExpressionTranslator(queryContext, queryCompiler),
                [ExpressionType.MemberAccess] = new MemberExpressionTranslator(queryContext, queryCompiler),
                [ExpressionType.Constant] = new ConstantExpressionTranslator(queryContext, queryCompiler),
                [ExpressionType.TypeIs] = new TypeBinaryExpressionTranslator(queryContext, queryCompiler),
                [ExpressionType.Parameter] = new ParameterExpressionTranslator(queryContext, queryCompiler),
                [ExpressionType.New] = new NewExpressionTranslator(queryContext, queryCompiler),
                [ExpressionType.MemberInit] = new MemberInitExpressionTranslator(queryContext, queryCompiler)
            };
        }

        protected void RegisterTranslator(ExpressionType expressionType, ITranslator translaor)
        {
            if (_nodeTypeTranslators == null)
            {
                Initialize(translaor.QueryContext, translaor.QueryCompiler);
            }

            _nodeTypeTranslators.AddOrUpdate(expressionType, translaor, (k, v) => translaor);
        }
    }
}
