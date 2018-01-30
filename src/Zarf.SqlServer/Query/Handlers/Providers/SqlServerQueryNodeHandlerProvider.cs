using System.Linq.Expressions;
using Zarf.Query;
using Zarf.Query.Handlers;
using Zarf.SqlServer.Query.Handlers.NodeTypes;

namespace Zarf.SqlServer.Query.Handlers
{
    public class SqlServerQueryNodeHandlerProvider : DefaultQueryNodeHanlderProvider
    {
        protected override void Initialize(IQueryContext queryContext, IQueryCompiler queryCompiler)
        {
            base.Initialize(queryContext, queryCompiler);

            RegisterHandler(ExpressionType.Call, new SqlServerMethodCallNodeHandler(queryContext, queryCompiler));
        }
    }
}
