using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public class MappingProvider
    {
        private Dictionary<Expression, IMapping> _maps = new Dictionary<Expression, IMapping>();

        public void Map(Expression source, Expression node, int order)
        {
            _maps[node] = new MemberMapping(source, node, order);
        }

        public IMapping GetMapping(Expression node)
        {
            if (_maps.TryGetValue(node, out IMapping mapping))
            {
                return mapping;
            }

            return null;
        }
    }
}
