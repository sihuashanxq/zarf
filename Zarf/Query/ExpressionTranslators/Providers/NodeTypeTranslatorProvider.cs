using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Queries.ExpressionTranslators.NodeTypes;

namespace Zarf.Queries.ExpressionTranslators
{
    public class NodeTypeTranslatorProvider : ITransaltorProvider
    {
        private Dictionary<ExpressionType, ITranslaor> _nodeTypeTranslators;

        public NodeTypeTranslatorProvider(IQueryContext queryContext,IQueryCompiler queryCompiler)
        {
            _nodeTypeTranslators = new Dictionary<ExpressionType, ITranslaor>();
            _nodeTypeTranslators[ExpressionType.Call] = new MethodCallExpressionTranslator(queryContext, queryCompiler);
            _nodeTypeTranslators[ExpressionType.MemberAccess] = new MemberExpressionTranslator(queryContext, queryCompiler);
            _nodeTypeTranslators[ExpressionType.Constant] = new ConstantExpressionTranslator(queryContext, queryCompiler);
            _nodeTypeTranslators[ExpressionType.TypeIs] = new TypeBinaryExpressionTranslator(queryContext, queryCompiler);
            _nodeTypeTranslators[ExpressionType.Parameter] = new ParameterExpressionTranslator(queryContext, queryCompiler);
            _nodeTypeTranslators[ExpressionType.New] = new NewExpressionTranslator(queryContext, queryCompiler);
            _nodeTypeTranslators[ExpressionType.MemberInit] = new MemberInitExpressionTranslator(queryContext, queryCompiler);
        }

        public ITranslaor GetTranslator(Expression node)
        {
            if (_nodeTypeTranslators.TryGetValue(node.NodeType, out ITranslaor translator))
            {
                return translator;
            }

            return null;
        }
    }
}
