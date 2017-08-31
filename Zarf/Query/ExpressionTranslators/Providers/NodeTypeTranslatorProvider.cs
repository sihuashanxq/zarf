using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Query.ExpressionTranslators.NodeTypes;

namespace Zarf.Query.ExpressionTranslators
{
    public class NodeTypeTranslatorProvider : ITransaltorProvider
    {
        private static Dictionary<ExpressionType, ITranslaor> _nodeTypeTranslators;

        public static ITransaltorProvider Default = new NodeTypeTranslatorProvider();

        static NodeTypeTranslatorProvider()
        {
            _nodeTypeTranslators = new Dictionary<ExpressionType, ITranslaor>();
            _nodeTypeTranslators[ExpressionType.Call] = new MethodCallExpressionTranslator();
            _nodeTypeTranslators[ExpressionType.MemberAccess] = new MemberExpressionTranslator();
            _nodeTypeTranslators[ExpressionType.Constant] = new ConstantExpressionTranslator();
            _nodeTypeTranslators[ExpressionType.TypeIs] = new TypeBinaryExpressionTranslator();
            _nodeTypeTranslators[ExpressionType.Parameter] = new ParameterExpressionTranslator();
            _nodeTypeTranslators[ExpressionType.New] = new NewExpressionTranslator();
            _nodeTypeTranslators[ExpressionType.MemberInit] = new MemberInitExpressionTranslator();
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
