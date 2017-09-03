using System.Reflection;
using Zarf.Query.Expressions;
using System.Collections.Generic;

namespace Zarf.Query
{
    public interface IPropertyNavigationContext
    {
        void AddPropertyNavigation(MemberInfo memberInfo, PropertyNavigation propertyNavigation);

        bool IsPropertyNavigation(MemberInfo memberInfo);

        PropertyNavigation GetNavigation(MemberInfo memberInfo);

        PropertyNavigation GetLastNavigation();
    }
}
