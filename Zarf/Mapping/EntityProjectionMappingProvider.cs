using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping
{
    public class EntityProjectionMappingProvider : IEntityProjectionMappingProvider
    {
        private Dictionary<Expression, IEntityProjectionMapping> _maps = new Dictionary<Expression, IEntityProjectionMapping>();

        private Dictionary<MemberInfo, int> __maps = new Dictionary<MemberInfo, int>();

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

        public int GetOrdinal(MemberInfo member)
        {
            if (__maps.ContainsKey(member))
            {
                return __maps[member];
            }

            return -1;
        }

        public void Map(MemberInfo member, int ordinal)
        {
            __maps[member] = ordinal;
        }
    }
}