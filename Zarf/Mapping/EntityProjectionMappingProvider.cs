using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public class EntityProjectionMappingProvider : IEntityProjectionMappingProvider
    {
        private Dictionary<Expression, ColumnDescriptor> _cahces = new Dictionary<Expression, ColumnDescriptor>();

        public void Map(ColumnDescriptor item)
        {
            _cahces[item.Expression] = item;
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