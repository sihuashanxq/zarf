using System.Collections.Generic;

namespace Zarf.Update
{
    public interface IEntityEntryCache
    {
        void AddOrUpdate(EntityEntry entry);

        IEnumerable<EntityEntry> GetCahcedEntries();

        void Clear();
    }
}
