using System.Collections.Generic;
using Zarf.Query.Expressions;

namespace Zarf.Query.Internals
{
    public interface IQueryColumnCaching
    {
        void AddColumn(ColumnExpression column);

        void AddColumnRange(IEnumerable<ColumnExpression> columns);

        ColumnExpression GetColumn(QueryColumnCacheKey cacheKey);
    }
}
