using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public class QueryColumnOrdinalMapper : IQueryColumnOrdinalMapper
    {
        private ConcurrentDictionary<Expression, ColumnDescriptor> _cahces = new ConcurrentDictionary<Expression, ColumnDescriptor>();

        public void Map(ColumnDescriptor item)
        {
            _cahces.AddOrUpdate(item.Expression, item, (key, o) => item);
        }

        public int GetOrdinal(Expression node)
        {
            if (_cahces.TryGetValue(node, out ColumnDescriptor v))
            {
                return v.Ordinal;
            }

            return -1;
        }

        public bool IsMapped(Expression node)
        {
            return _cahces.ContainsKey(node);
        }
    }
}