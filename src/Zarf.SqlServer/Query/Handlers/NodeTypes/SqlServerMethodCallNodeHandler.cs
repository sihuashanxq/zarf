using Zarf.Query;
using Zarf.Query.Handlers;
using Zarf.Query.Handlers.NodeTypes;
using Zarf.SqlServer.Query.Handlers.NodeTypes.MethodCalls;

namespace Zarf.SqlServer.Query.Handlers.NodeTypes
{
    public class SqlServerMethodCallNodeHandler : MethodCallNodeHandler
    {
        public SqlServerMethodCallNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
            RegisterHandler(SqlServerSkipNodeHandler.SupprotedMethods, new SqlServerSkipNodeHandler(queryContext, queryCompiper));
        }

        protected override IQueryNodeHandler GetNameNodeHanlder()
        {
            return new SqlServerMethodNameNodeHandler(QueryContext, QueryCompiler);
        }
    }
}
