using System.Linq.Expressions;
using System.Collections.Generic;

namespace Zarf.Query.Internals
{
    public interface IQueryProjectionMapper
    {
        void Map(Expression expression, Expression maped);

        Expression GetMappedExpression(Expression expression);

        IEnumerable<KeyValuePair<Expression, Expression>> GetAllExpressions();
    }
}
