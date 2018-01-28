using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zarf.Core;
using Zarf.Query;
using Zarf.Update;

namespace Zarf
{
    public class DbContext : IDisposable, IServiceScope
    {
        private int _immInsertRowsCount;

        public string ConnectionString => DbService.ConnectionString;

        public IServiceProvider ServiceProvider { get; }

        public IDbService DbService { get; }

        public IDbModifyExecutor DbModifyExecutor => GetService<IDbModifyExecutor>();

        public IEntityTracker Tracker => GetService<IEntityTracker>();

        public IEntityEntryCache EntryEntryCache => GetService<IEntityEntryCache>();

        public IQueryContextFactory QueryContextFactory => GetService<IQueryContextFactory>();

        public IQueryExecutor QueryExecutor => GetService<IQueryExecutor>();

        public DbContext(Func<IDbServiceBuilder, IDbService> serviceBuilder)
            : this(serviceBuilder(null))
        {

        }

        public DbContext(IDbService dbService)
        {
            DbService = dbService;
            ServiceProvider = DbService.ServiceProvder;
        }

        protected TService GetService<TService>()
        {
            return ServiceProvider.GetService<TService>();
        }

        public IQuery<TEntity> Query<TEntity>()
        {
            return new Query<TEntity>(this, ServiceProvider.GetService<IQueryExecutor>());
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
                    EntryEntryCache.AddOrUpdate(entry);
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
            EntryEntryCache.AddOrUpdate(EntityEntry.Create(entity, EntityState.Update));
        }

        public virtual Task UpdateAsync<TEntity>(TEntity entity)
        {
            return Task.Run(() => Update<TEntity, TEntity>(entity, null));
        }

        public virtual void Delete<TEntity>(TEntity entity) => Delete<TEntity, TEntity>(entity, null);

        public virtual void Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            EntryEntryCache.AddOrUpdate(EntityEntry.Create(entity, EntityState.Delete));
        }

        public virtual Task DeleteAsync<TEntity>(TEntity entity)
        {
            return Task.Run(() => Delete(entity));
        }

        public IDbEntityTransaction BeginTransaction()
        {
            if (!DbService.EntityConnection.HasTransaction())
            {
                Save();
            }

            return DbService.EntityConnection.BeginTransaction();
        }

        public Task<IDbEntityTransaction> BeginTransactionAsync()
        {
            return Task.FromResult(BeginTransaction());
        }

        public int Save()
        {
            var rowsCount = _immInsertRowsCount;
            var entries = EntryEntryCache.GetCahcedEntries().ToList();
            _immInsertRowsCount = 0;

            if (entries.Count == 0)
            {
                return rowsCount;
            }

            return rowsCount + Save(entries);
        }

        private int Save(IEnumerable<EntityEntry> entries)
        {
            if (!DbService.EntityConnection.HasTransaction())
            {
                using (var transaction = DbService.EntityConnection.BeginTransaction())
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
            var entries = EntryEntryCache.GetCahcedEntries().ToList();
            if (entries == null)
            {
                return await Task.FromResult(0);
            }

            if (!DbService.EntityConnection.HasTransaction())
            {
                using (var transaction = DbService.EntityConnection.BeginTransaction())
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
            EntryEntryCache.Clear();
            Tracker.Clear();
            DbService.EntityConnection.Dispose();
        }
    }
}
