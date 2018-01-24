using Zarf.Query;
using Zarf.Query.ExpressionTranslators.NodeTypes;
using Zarf.SqlServer.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.SqlServer.Query.ExpressionTranslators.NodeTypes
{
    public class SqlServerMethodCallTranslator : MethodCallExpressionTranslator
    {
        public SqlServerMethodCallTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
            SubQueryTranslator = new SqlserverSubQueryTranslator(queryContext, queryCompiper);
            RegisterTranslator(SqlServerSkipTranslator.SupprotedMethods, new SqlServerSkipTranslator(queryContext, queryCompiper));
        }
    }
}
