using System;
using System.Collections.Generic;
using System.Reflection;
using Zarf.Entities;

namespace Zarf.Mapping
{
    public class TypeDescriptor
    {
        public Type Type { get; }

        public List<IMemberDescriptor> MemberDescriptors { get; }

        public ConstructorInfo Constructor { get; }

        public TypeDescriptor(Type typeOfEntity, List<IMemberDescriptor> memberDescriptors)
        {
            Type = typeOfEntity;
            MemberDescriptors = memberDescriptors;
            Constructor = typeOfEntity.GetConstructor(Type.EmptyTypes);
        }
    }
}
