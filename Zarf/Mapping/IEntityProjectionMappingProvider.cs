using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping
{
    public interface IEntityProjectionMappingProvider
    {
        void Map(Expression refrenceProjection, Expression source, int ordinal);

        IEntityProjectionMapping GetMapping(Expression refrenceProjection);

        int GetOrdinal(MemberInfo member);

        int GetOrdinal(Expression bindExpression);

        void Map(MemberInfo member, int ordinal);
    }
}
