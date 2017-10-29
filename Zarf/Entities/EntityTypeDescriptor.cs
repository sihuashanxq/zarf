using System;
using System.Reflection;
using System.Collections.Generic;
using Zarf.Extensions;

namespace Zarf.Mapping
{
    public class EntityTypeDescriptor
    {
        public Type Type { get; }

        public List<MemberInfo> Members { get; }

        public ConstructorInfo Constructor { get; }

        public EntityTypeDescriptor(Type entityType)
        {
            Type = entityType;
            Members = new List<MemberInfo>();
            Constructor = entityType.GetConstructor(Type.EmptyTypes);
        }

        public IEnumerable<MemberInfo> GetExpandMembers()
        {
            foreach (var item in Members)
            {
                if (item.Is<PropertyInfo>() && item.Cast<PropertyInfo>().SetMethod != null)
                {
                    yield return item;
                }
                else
                {
                    yield return item;
                }
            }
        }
    }
}
