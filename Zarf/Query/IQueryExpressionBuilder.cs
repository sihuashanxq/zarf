using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public interface IQueryExpressionBuilder
    {
        Expression Build(Expression linqExpression, QueryContext context);
    }
}
