using System.Collections.Concurrent;
using System.Collections.Generic;
using Zarf.Query.Expressions;

namespace Zarf.Query.Internals
{
    public class QueryColumnCaching : IQueryColumnCaching
    {
        protected ConcurrentDictionary<QueryColumnCacheKey, ColumnExpression> Caches { get; }

        public QueryColumnCaching()
        {
            Caches = new ConcurrentDictionary<QueryColumnCacheKey, ColumnExpression>();
        }

        public void AddColumn(ColumnExpression column)
        {
            var cacheKey = new QueryColumnCacheKey(column.Query, column.Member);
            Caches.AddOrUpdate(cacheKey, column, (key, o) => column);
        }

        public void AddColumnRange(IEnumerable<ColumnExpression> columns)
        {
            foreach (var item in columns)
            {
                AddColumn(item);
            }
        }

        public ColumnExpression GetColumn(QueryColumnCacheKey cacheKey)
        {
            return Caches.TryGetValue(cacheKey, out var col) ? col : null;
        }
    }
}
