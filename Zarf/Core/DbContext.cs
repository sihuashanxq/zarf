using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Update;
using Zarf.Update.Executors;

namespace Zarf
{
    public abstract class DbContext : IDisposable
    {
        public IDbContextParts DbContextParts { get; }

        public IDbModifyExecutor DbModifyExecutor { get; }

        public IEntityTracker Tracker { get; }

        public IEntityEntryCache CacheEntryStore { get; }

        public DbContext(IDbContextParts dbContextParts)
        {
            DbContextParts = dbContextParts;
            Tracker = new EntityTracker();
            CacheEntryStore = new EntityEntryCache();
            DbModifyExecutor = new DbModifyExecutor(DbContextParts.CommandFacotry, dbContextParts.SqlBuilder, Tracker);
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
            if (entities == null)
            {
                return;
            }

            //自增长列,立即执行
            var immediates = new List<EntityEntry>();
            foreach (var entity in entities)
            {
                var entry = EntityEntry.Create(entity, EntityState.Insert);
                if (entry.AutoIncrementProperty != null)
                {
                    immediates.Add(entry);
                }
                else
                {
                    CacheEntryStore.AddOrUpdate(entry);
                }
            }

            if (immediates.Count != 0)
            {
                Flush(immediates);
            }
        }

        public virtual void Add<TEntity>(TEntity entity) => AddRange(new[] { entity });

        public virtual void Add(object entity) => Add<object>(entity);

        public virtual void Update<TEntity>(TEntity entity) => Update<TEntity, TEntity>(entity, null);

        public virtual void Update(object entity) => Update<object>(entity);

        public virtual void Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            CacheEntryStore.AddOrUpdate(EntityEntry.Create(entity, EntityState.Update));
        }

        public virtual void Delete<TEntity>(TEntity entity) => Delete<TEntity, TEntity>(entity, null);

        public virtual void Delete(object entity) => Delete<object>(entity);

        public virtual void Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            CacheEntryStore.AddOrUpdate(EntityEntry.Create(entity, EntityState.Delete));
        }

        public void Flush()
        {
            var entries = CacheEntryStore.GetCahcedEntries().ToList();
            if (entries.Count == 0)
            {
                return;
            }

            Flush(entries);
        }

        private void Flush(IEnumerable<EntityEntry> entries)
        {
            DbModifyExecutor.Execute(entries);
        }

        public void Dispose()
        {
            Flush();
            CacheEntryStore.Clear();
            Tracker.Clear();
        }
    }
}
