using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Mapping
{
    public interface IEntityProjectionMappingProvider
    {
        IEntityProjectionMapping GetMapping(Expression refrenceProjection);

        void Map(Expression bindExpression, Expression source, int ordinal);

        int GetOrdinal(Expression query, MemberInfo member);

        int GetOrdinal(Expression query, Expression bindExpression);

        void Map(Projection projection);
    }
}
