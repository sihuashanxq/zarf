using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Mapping
{
    public class EntityProjectionMappingProvider : IEntityProjectionMappingProvider
    {
        private Dictionary<Expression, IEntityProjectionMapping> _maps = new Dictionary<Expression, IEntityProjectionMapping>();

        private Dictionary<MemberInfo, int> __maps = new Dictionary<MemberInfo, int>();

        private Dictionary<Expression, int> __maps2 = new Dictionary<Expression, int>();

        private Dictionary<Expression, HashSet<Projection>> _maps4 = new Dictionary<Expression, HashSet<Projection>>();

        private Dictionary<Projection, int> __maps5 = new Dictionary<Projection, int>();

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

        public void Map(Projection projection)
        {
            if (!_maps4.ContainsKey(projection.Query))
            {
                _maps4[projection.Query] = new HashSet<Projection>();
            }

            _maps4[projection.Query].Add(projection);

            __maps5[projection] = projection.Ordinal;
        }

        public int GetOrdinal(Expression query, MemberInfo member, Expression exp)
        {
            var p = new Projection()
            {
                Query = query as FromTableExpression,
                Member = member,
                Expression = exp
            };

            if (__maps5.ContainsKey(p))
            {
                var i = __maps5[p];
                return i;
            }
            return -1;
        }

        public int GetOrdinal(Expression query, MemberInfo member)
        {
            if (!_maps4.ContainsKey(query))
            {
                return -1;
            }

            foreach (var item in _maps4[query])
            {
                if (item.Member == member)
                {
                    return item.Ordinal;
                }
            }

            return -1;
        }

        public int GetOrdinal(Expression query, Expression bindExpression)
        {
            if (!_maps4.ContainsKey(query))
            {
                return -1;
            }

            foreach (var item in _maps4[query])
            {
                if (item.Expression.Equals(bindExpression))
                {
                    return item.Ordinal;
                }
            }

            return -1;
        }
    }
}