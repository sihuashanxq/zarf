using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.NodeTypes;
using Zarf.SqlServer.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.SqlServer.Query.ExpressionTranslators.NodeTypes
{
    public class SqlserverSubQueryTranslator : SubQueryTranslator
    {
        public SqlserverSubQueryTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression expression, MethodInfo method)
        {
            if (method.Name == "Skip")
            {
                return new SqlServerSkipTranslator(QueryContext, QueryCompiler).Translate(select, expression, method);
            }

            return base.Translate(select, expression, method);
        }
    }
}
