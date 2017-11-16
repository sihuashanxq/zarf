using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Zarf.Update
{
    public class EntityEntryCache : IEntityEntryCache
    {
        private ConcurrentDictionary<object, EntityEntry> _entries;

        public EntityEntryCache()
        {
            _entries = new ConcurrentDictionary<object, EntityEntry>();
        }

        public void AddOrUpdate(EntityEntry entry)
        {
            _entries.AddOrUpdate(entry.Entity, entry, (k, e) =>
            {
                if (entry.State == EntityState.Delete && e.State == EntityState.Insert)
                {
                    return null;
                }

                e.Entity = entry.Entity;
                return e;
            });
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public IEnumerable<EntityEntry> GetCahcedEntries()
        {
            foreach (var key in _entries.Keys)
            {
                if (_entries.TryRemove(key, out var entry))
                {
                    yield return entry;
                }
            }
        }
    }
}
