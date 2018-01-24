using System.Linq.Expressions;
using Zarf.Query;
using Zarf.Query.ExpressionTranslators;
using Zarf.SqlServer.Query.ExpressionTranslators.NodeTypes;

namespace Zarf.SqlServer.Query.ExpressionTranslators
{
    public class SqlServerNodeTypeTranslatorProvider : NodeTypeTranslatorProvider
    {
        protected override void Initialize(IQueryContext queryContext, IQueryCompiler queryCompiler)
        {
            base.Initialize(queryContext, queryCompiler);

            RegisterTranslator(ExpressionType.Call, new SqlServerMethodCallTranslator(queryContext, queryCompiler));
        }
    }
}
