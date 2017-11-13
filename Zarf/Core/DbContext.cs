using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Zarf.Core;
using Zarf.Extensions;
using Zarf.Update;
using Zarf.Update.Executors;

namespace Zarf
{
    public interface IEntityCacheEntryStore
    {
        void AddOrUpdate(EntityEntry entry);

        IEnumerable<EntityEntry> GetCahcedEntries();

        void Clear();
    }

    public class EntityEntryCacheStore : IEntityCacheEntryStore
    {
        private ConcurrentDictionary<object, EntityEntry> _entries;

        public EntityEntryCacheStore()
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
            //which time to call
            _entries.Clear();
        }

        public IEnumerable<EntityEntry> GetCahcedEntries()
        {
            return _entries.Values;
        }
    }

    public abstract class DbContext : IDisposable
    {
        public IDbContextParts DbContextParts { get; }

        public IDbCommandExecutor DbModifyCommandExecutor { get; }

        public IEntityTracker Tracker { get; }

        public IEntityCacheEntryStore CacheEntryStore { get; }

        public DbContext(IDbContextParts dbContextParts)
        {
            DbContextParts = dbContextParts;
            Tracker = new EntityTracker();
            CacheEntryStore = new EntityEntryCacheStore();
            DbModifyCommandExecutor = new CompositeDbCommandExecutor(DbContextParts.CommandFacotry, dbContextParts.SqlBuilder, Tracker);
        }

        public IDbQuery<TEntity> Query<TEntity>()
        {
            return new DbQuery<TEntity>(new DbQueryProvider(this));
        }

        public IDbQuery<TEntity> Query<TEntity>(string queryText)
        {
            return null;
        }

        public void TrackEntity<TEntity>(TEntity entity)
            where TEntity : class
        {
            Tracker.TrackEntity(entity);
        }

        public virtual void AddRange(IEnumerable<object> entities) => AddRange<object>(entities);

        public virtual void AddRange<TEntity>(IEnumerable<TEntity> entities)
        {
            if (entities == null || entities.Count() == 0)
            {
                return;
            }

            var entries = new List<EntityEntry>();
            foreach (var entity in entities)
            {
                var entry = EntityEntry.Create(entity, EntityState.Insert);
                entries.Add(entry);
                CacheEntryStore.AddOrUpdate(entry);
            }

            //DbModifyCommandExecutor.Execute(new DbModifyOperation(entries, null));
        }

        public virtual void Add<TEntity>(TEntity entity) => AddRange(new[] { entity });

        public virtual void Add(object entity) => Add<object>(entity);

        public virtual void Update<TEntity>(TEntity entity) => Update<TEntity, TEntity>(entity, null);

        public virtual void Update(object entity) => Update<object>(entity);

        public virtual void Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            var entry = EntityEntry.Create(entity, EntityState.Update);
            CacheEntryStore.AddOrUpdate(entry);
            return;
            //return DbModifyCommandExecutor.Execute(new DbModifyOperation(new[] { entry }, GetIdentityMember(identity)));
        }

        public virtual void Delete<TEntity>(TEntity entity) => Delete<TEntity, TEntity>(entity, null);

        public virtual void Delete(object entity) => Delete<object>(entity);

        public virtual void Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            var entry = EntityEntry.Create(entity, EntityState.Delete);
            CacheEntryStore.AddOrUpdate(entry);
            return;
            //return DbModifyCommandExecutor.Execute(new DbModifyOperation(new[] { entry }, GetIdentityMember(identity)));
        }

        public void Flush()
        {
            var entries = CacheEntryStore.GetCahcedEntries().ToList();
            if (entries.Count == 0)
            {
                return;
            }

            DbModifyCommandExecutor.Execute(entries);
        }

        public void Dispose()
        {

        }
    }
}
