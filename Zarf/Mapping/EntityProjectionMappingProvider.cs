using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public class EntityProjectionMappingProvider : IEntityProjectionMappingProvider
    {
        private Dictionary<Expression, Projection> _cahces = new Dictionary<Expression, Projection>();

        public void Map(Projection item)
        {
            _cahces[item.Expression] = item;
        }

        public int GetOrdinal(Expression node)
        {
            if (_cahces.TryGetValue(node, out Projection v))
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