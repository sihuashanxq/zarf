using System.Collections.Concurrent;
using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Internals
{
    public class ExpressionMapper : IExpressionMapper
    {
        protected ConcurrentDictionary<Expression, Expression> Maps { get; }

        public ExpressionMapper()
        {
            Maps = new ConcurrentDictionary<Expression, Expression>(new ExpressionEqualityComparer());
        }

        public void Map(Expression key, Expression value)
        {
            Maps.AddOrUpdate(key, value, (k, v) => value);
        }

        public Expression GetMappedExpression(Expression key)
        {
            return Maps.TryGetValue(key, out var v) ? v : default(Expression);
        }
    }
}
