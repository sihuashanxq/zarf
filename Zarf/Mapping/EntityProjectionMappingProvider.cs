using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Mapping
{
    public class EntityProjectionMappingProvider : IEntityProjectionMappingProvider
    {
        private Dictionary<Expression, IEntityProjectionMapping> _maps = new Dictionary<Expression, IEntityProjectionMapping>();

        private Dictionary<MemberInfo, int> __maps = new Dictionary<MemberInfo, int>();

        private Dictionary<Expression, int> __maps2 = new Dictionary<Expression, int>();

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

        public void Map(Projection projection, int ordinal)
        {
            __maps[projection.Member] = ordinal;
            __maps2[projection.Expression] = ordinal;
        }

        public int GetOrdinal(Expression node)
        {
            if (__maps2.ContainsKey(node))
            {
                return __maps2[node];
            }

            return -1;
        }

        public void Map(MemberInfo member, int ordinal)
        {
            throw new System.NotImplementedException();
        }
    }
}