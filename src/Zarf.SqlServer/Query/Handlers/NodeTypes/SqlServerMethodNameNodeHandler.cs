using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query;
using Zarf.Query.Expressions;
using Zarf.Query.Handlers.NodeTypes;
using Zarf.SqlServer.Query.Handlers.NodeTypes.MethodCalls;

namespace Zarf.SqlServer.Query.Handlers.NodeTypes
{
    public class SqlServerMethodNameNodeHandler : DefaultMethodCallNameNodeHandler
    {
        public SqlServerMethodNameNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression HandleNode(SelectExpression select, Expression expression, MethodInfo method)
        {
            if (method.Name == "Skip")
            {
                return new SqlServerSkipNodeHandler(QueryContext, QueryCompiler).HandleNode(select, expression, method);
            }

            return base.HandleNode(select, expression, method);
        }
    }
}
