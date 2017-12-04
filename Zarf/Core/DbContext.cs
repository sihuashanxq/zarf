using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        public IQuery<TEntity> Query<TEntity>()
        {
            return new Query<TEntity>(this);
        }

        public void TrackEntity<TEntity>(TEntity entity)
            where TEntity : class
        {
            Tracker.TrackEntity(entity);
        }

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
                _immInsertRowsCount += Save(immediates);
            }
        }

        public virtual Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        {
            return Task.Run(() => AddRange(entities));
        }

        public virtual void Add<TEntity>(TEntity entity) => AddRange(new[] { entity });

        public virtual Task AddAsync<TEntity>(TEntity entity)
        {
            return Task.Run(() => Add(entity));
        }

        public virtual void Update<TEntity>(TEntity entity) => Update<TEntity, TEntity>(entity, null);

        public virtual void Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            EntryCache.AddOrUpdate(EntityEntry.Create(entity, EntityState.Update));
        }

        public virtual Task UpdateAsync<TEntity>(TEntity entity)
        {
            return Task.Run(() => Update<TEntity, TEntity>(entity, null));
        }

        public virtual void Delete<TEntity>(TEntity entity) => Delete<TEntity, TEntity>(entity, null);

        public virtual void Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            EntryCache.AddOrUpdate(EntityEntry.Create(entity, EntityState.Delete));
        }

        public virtual Task DeleteAsync<TEntity>(TEntity entity)
        {
            return Task.Run(() => Delete(entity));
        }

        public IDbEntityTransaction BeginTransaction()
        {
            if (!DbContextParts.EntityConnection.HasTransaction())
            {
                Save();
            }

            return DbContextParts.EntityConnection.BeginTransaction();
        }

        public Task<IDbEntityTransaction> BeginTransactionAsync()
        {
            return Task.FromResult(BeginTransaction());
        }

        public int Save()
        {
            var rowsCount = _immInsertRowsCount;
            var entries = EntryCache.GetCahcedEntries().ToList();
            _immInsertRowsCount = 0;

            if (entries.Count == 0)
            {
                return rowsCount;
            }

            return rowsCount + Save(entries);
        }

        private int Save(IEnumerable<EntityEntry> entries)
        {
            if (!DbContextParts.EntityConnection.HasTransaction())
            {
                using (var transaction = DbContextParts.EntityConnection.BeginTransaction())
                {
                    try
                    {
                        return DbModifyExecutor.Execute(entries);
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw e;
                    }
                }
            }
            else
            {
                return DbModifyExecutor.Execute(entries);
            }
        }

        public async Task<int> SaveAsync()
        {
            var entries = EntryCache.GetCahcedEntries().ToList();
            if (entries == null)
            {
                return await Task.FromResult(0);
            }

            if (!DbContextParts.EntityConnection.HasTransaction())
            {
                using (var transaction = DbContextParts.EntityConnection.BeginTransaction())
                {
                    try
                    {
                        return await DbModifyExecutor.ExecuteAsync(entries);
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw e;
                    }
                }
            }
            else
            {
                return await DbModifyExecutor.ExecuteAsync(entries);
            }
        }

        public void Dispose()
        {
            Save();
            EntryCache.Clear();
            Tracker.Clear();
            DbContextParts.EntityConnection.Dispose();
        }
    }
}
