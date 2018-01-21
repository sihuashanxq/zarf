using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Queries.Expressions;

namespace Zarf.Queries.Internals
{
    public class QueryProjectionMapper : IQueryProjectionMapper
    {
        protected ConcurrentDictionary<Expression, Expression> Maps { get; }

        public QueryProjectionMapper()
        {
            Maps = new ConcurrentDictionary<Expression, Expression>(new ExpressionEqualityComparer());
        }

        public void Map(Expression expression, Expression mapped)
        {
            Maps.AddOrUpdate(expression, mapped, (k, v) => mapped);
        }

        public Expression GetMappedExpression(Expression expression)
        {
            return Maps.TryGetValue(expression, out var v) ? v : default(Expression);
        }

        public IEnumerable<KeyValuePair<Expression, Expression>> GetAllExpressions()
        {
            return Maps.ToArray();
        }
    }
}
