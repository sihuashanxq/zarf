using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public class EntityProjectionMappingProvider : IEntityProjectionMappingProvider
    {
        private Dictionary<Expression, IEntityProjectionMapping> _maps = new Dictionary<Expression, IEntityProjectionMapping>();

        public void Map(Expression refrenceProjection, Expression source, int ordinal)
        {
            _maps[refrenceProjection] = new EntityProjectionMapping(source, refrenceProjection, null, ordinal);
        }

        public IEntityProjectionMapping GetMapping(Expression refrenceProjection)
        {
            if (_maps.TryGetValue(refrenceProjection, out IEntityProjectionMapping mapping))
            {
                return mapping;
            }

            return null;
        }
    }
}