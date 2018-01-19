using System.Reflection;
using System.Collections.Generic;

namespace Zarf.Queries
{
    //LOCK TODO
    public class PropertyNavigationContext : IPropertyNavigationContext
    {
        protected virtual Dictionary<MemberInfo, PropertyNavigation> Navigations { get; set; }

        protected virtual MemberInfo LastAddedProperty { get; set; }

        public PropertyNavigationContext()
        {
            Navigations = new Dictionary<MemberInfo, PropertyNavigation>();
        }

        public void AddPropertyNavigation(MemberInfo memberInfo, PropertyNavigation propertyNavigation)
        {
            LastAddedProperty = memberInfo;
            Navigations[memberInfo] = propertyNavigation;
        }

        public bool IsPropertyNavigation(MemberInfo memberInfo)
        {
            return Navigations.ContainsKey(memberInfo);
        }

        public PropertyNavigation GetNavigation(MemberInfo memberInfo)
        {
            if (IsPropertyNavigation(memberInfo))
            {
                return Navigations[memberInfo];
            }

            return null;
        }

        public PropertyNavigation GetLastNavigation()
        {
            return GetNavigation(LastAddedProperty);
        }
    }
}
