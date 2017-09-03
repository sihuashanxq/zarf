using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query
{
    public interface IEntityMemberSourceMappingProvider
    {
        void Map(MemberInfo memberInfo, Expression mapExpression);

        bool IsMapped(MemberInfo memberInfo);

        Expression GetExpression(MemberInfo memberInfo);

        void UpdateExpression(Expression oldMapExpression, Expression newMapExpression);
    }
}
