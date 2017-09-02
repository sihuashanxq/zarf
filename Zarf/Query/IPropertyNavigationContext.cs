using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public interface IPropertyNavigationContext
    {
        IQuerySourceProvider QuerySourceProvider { get; }

        void AddPropertyNavigation(MemberInfo memberInfo, QueryExpression queryExpression);

        bool IsPropertyNavigation(MemberInfo memberInfo);

        QueryExpression GetNavigationExpression(MemberInfo memberInfo);
    }
}
