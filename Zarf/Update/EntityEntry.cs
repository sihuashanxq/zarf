using System;
using System.Collections.Generic;
using System.Linq;
using Zarf.Entities;
using Zarf.Mapping;
using Zarf.Update;

namespace Zarf
{
    public class EntityEntry
    {
        public object Entity { get; set; }

        public Type Type { get; }

        public EntityState State { get; }

        public IEnumerable<IMemberDescriptor> Members { get; }

        public IMemberDescriptor AutoIncrementProperty => Members?.FirstOrDefault(item => item.IsAutoIncrement);

        public IMemberDescriptor ConventionId => Members?.FirstOrDefault(item => item.Member.Name.ToLower() == "id");

        public IMemberDescriptor Primary => Members?.FirstOrDefault(item => item.IsPrimaryKey) ?? AutoIncrementProperty ?? ConventionId;

        public EntityEntry(object entity, EntityState state, IEnumerable<IMemberDescriptor> members)
        {
            Entity = entity;
            State = state;
            Members = members;
            Type = entity.GetType();
        }

        public static EntityEntry Create(object entity, EntityState state)
        {
            return new EntityEntry(entity, state, TypeDescriptorCacheFactory.Factory.Create(entity.GetType()).MemberDescriptors);
        }
    }
}
