using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
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

        public IEntityEntryCache EntryCache { get; }

        private int _immInsertRowsCount;

        public DbContext(IDbContextParts dbContextParts)
        {
            DbContextParts = dbContextParts;
            Tracker = new EntityTracker();
            EntryCache = new EntityEntryCache();
            DbModifyExecutor = new DbModifyExecutor(DbContextParts.EntityCommandFacotry, dbContextParts.CommandTextBuilder, Tracker);
        }

        public IDbQuery<TEntity> Query<TEntity>()
        {
            return new DbQuery<TEntity>(new DbQueryProvider(this));
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
                    EntryCache.AddOrUpdate(entry);
                }
            }

            if (immediates.Count != 0)
            {
                _immInsertRowsCount += Flush(immediates);
            }
        }

        public virtual void Add<TEntity>(TEntity entity) => AddRange(new[] { entity });

        public virtual void Add(object entity) => Add<object>(entity);

        public virtual void Update<TEntity>(TEntity entity) => Update<TEntity, TEntity>(entity, null);

        public virtual void Update(object entity) => Update<object>(entity);

        public virtual void Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            EntryCache.AddOrUpdate(EntityEntry.Create(entity, EntityState.Update));
        }

        public virtual void Delete<TEntity>(TEntity entity) => Delete<TEntity, TEntity>(entity, null);

        public virtual void Delete(object entity) => Delete<object>(entity);

        public virtual void Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            EntryCache.AddOrUpdate(EntityEntry.Create(entity, EntityState.Delete));
        }

        public IDbEntityTransaction BeginTransaction()
        {
            Flush();
            return DbContextParts.EntityConnection.BeginTransaction();
        }

        public int Flush()
        {
            var rowsCount = _immInsertRowsCount;
            var entries = EntryCache.GetCahcedEntries().ToList();
            _immInsertRowsCount = 0;

            if (entries.Count == 0)
            {
                return rowsCount;
            }

            return rowsCount + Flush(entries);
        }

        private int Flush(IEnumerable<EntityEntry> entries)
        {
            if (DbContextParts.EntityConnection.DbTransaction == null)
            {
                var transaction = DbContextParts.EntityConnection.BeginTransaction();
                try
                {
                    var rowsCount = DbModifyExecutor.Execute(entries);
                    transaction.Commit();
                    return rowsCount;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
            else
            {
                return DbModifyExecutor.Execute(entries);
            }
        }

        public void Dispose()
        {
            Flush();
            EntryCache.Clear();
            Tracker.Clear();
        }
    }
}
