using System;
using System.Collections.Generic;
using System.Linq;
using Zarf.Update;

namespace Zarf
{
    public class EntityEntry
    {
        public Type Type { get; }

        public object Entity { get; }

        public EntityState State { get; }

        public IEnumerable<MemberDescriptor> Members { get; }

        public MemberDescriptor Increment => Members?.FirstOrDefault(item => item.IsIncrement);

        public MemberDescriptor Primary => Members?.FirstOrDefault(item => item.IsPrimary);
    }

    public class DbModifyOperation
    {
        public IEnumerable<EntityEntry> Entities { get; }

        public MemberDescriptor Identity { get; }
    }

    public enum EntityState
    {
        Insert,
        Update,
        Delete
    }
}
