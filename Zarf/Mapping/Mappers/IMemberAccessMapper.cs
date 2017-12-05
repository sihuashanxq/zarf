using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query
{
    /// <summary>
    /// Map Member&Expression
    /// x.Id=ColumnExpression{Id}
    /// </summary>
    public interface IMemberAccessMapper
    {
        Expression GetMappedExpression(MemberInfo memberInfo);

        void Map(MemberInfo memberInfo, Expression mapExpression);

        bool IsMapped(MemberInfo memberInfo);
    }
}
