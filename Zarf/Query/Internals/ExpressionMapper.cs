using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Queries.Expressions;

namespace Zarf.Queries.Internals
{
    public class ExpressionMapper : IQueryProjectionMapper
    {
        protected ConcurrentDictionary<Expression, Expression> Maps { get; }

        public ExpressionMapper()
        {
            Maps = new ConcurrentDictionary<Expression, Expression>(new ExpressionEqualityComparer());
        }

        public void Map(Expression projection, Expression mappedProjection)
        {
            Maps.AddOrUpdate(projection, mappedProjection, (k, v) => mappedProjection);
        }

        public Expression GetMappedProjection(Expression projection)
        {
            return Maps.TryGetValue(projection, out var v) ? v : default(Expression);
        }

        public IEnumerable<KeyValuePair<Expression, Expression>> GetAllProjections()
        {
            return Maps.ToArray();
        }
    }
}
