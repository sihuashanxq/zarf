using System.Collections.Generic;

namespace Zarf.Update
{
    public class DbModifyOperation
    {
        public IEnumerable<EntityEntry> Entries { get; }

        public MemberDescriptor Identity { get; }

        public DbModifyOperation(IEnumerable<EntityEntry> entries, MemberDescriptor identity)
        {
            Entries = entries;
            Identity = identity;
        }
    }
}
