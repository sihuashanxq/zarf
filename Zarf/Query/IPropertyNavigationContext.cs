using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public interface IPropertyNavigationContext
    {
        void AddPropertyNavigation(MemberInfo memberInfo, PropertyNavigation propertyNavigation);

        bool IsPropertyNavigation(MemberInfo memberInfo);

        PropertyNavigation GetNavigationExpression(MemberInfo memberInfo);
    }
}
