using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    //LOCK TODO
    public class PropertyNavigationContext : IPropertyNavigationContext
    {
        protected virtual Dictionary<MemberInfo, PropertyNavigation> Navigations { get; set; }

        public void AddPropertyNavigation(MemberInfo memberInfo, PropertyNavigation propertyNavigation)
        {
            Navigations[memberInfo] = propertyNavigation;
        }

        public bool IsPropertyNavigation(MemberInfo memberInfo)
        {
            return Navigations.ContainsKey(memberInfo);
        }

        public PropertyNavigation GetNavigationExpression(MemberInfo memberInfo)
        {
            if (IsPropertyNavigation(memberInfo))
            {
                return Navigations[memberInfo];
            }

            return null;
        }
    }
}
