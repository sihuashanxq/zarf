using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Update;
using Zarf.Update.Commands;
using Zarf.Update.Executors;

namespace Zarf
{
    public interface IEntityEntryCache
    {
        void AddOrUpdate(EntityEntry entry);

        IEnumerable<EntityEntry> GetCahcedEntries();

        void Clear();
    }

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

        public IEntityEntryCache CacheEntryStore { get; }

        public DbContext(IDbContextParts dbContextParts)
        {
            DbContextParts = dbContextParts;
            Tracker = new EntityTracker();
            CacheEntryStore = new EntityEntryCache();
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

        public void Flush(FlushMode model = FlushMode.Default)
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

    public class EntityEntryStore
    {
        //TODO TO DbStore Expression 
        //Executor
        const int MaxParameterCount = 999;

        private int _colPostfix;

        private IEntityTracker _tracker;

        private List<DbModifyCommandGroup> _groups;

        public EntityEntryStore(IEntityTracker tracker)
        {
            _tracker = tracker;
            _groups = new List<DbModifyCommandGroup>();
        }

        public void Store(IEnumerable<EntityEntry> entries, FlushMode flushMode = FlushMode.Default)
        {
            foreach (var item in entries.OrderBy(item => item.State).ThenBy(item => item.Type.GetHashCode()))
            {
                switch (item.State)
                {
                    case EntityState.Insert:
                        AddInsertEntry(item, flushMode);
                        break;
                    case EntityState.Update:
                        AddUpdateEntry(item, flushMode);
                        break;
                    default:
                        AddDeleteEntry(item, flushMode);
                        break;
                }
            }
        }

        protected void AddInsertEntry(EntityEntry entry, FlushMode flushMode = FlushMode.Default)
        {
            var columns = new List<string>();
            var dbParams = new List<DbParameter>();

            foreach (var item in entry.Members)
            {
                if (item.IsIncrement)
                {
                    continue;
                }

                columns.Add(GetColumnName(item));
                dbParams.Add(new DbParameter(GetNewParameterName(), item.GetValue(entry.Entity)));
            }

            AddCommandToGroup(new DbInsertCommand(entry, columns, dbParams), flushMode);
        }

        protected void AddUpdateEntry(EntityEntry entry, FlushMode flushMode)
        {
            var columns = new List<string>();
            var paramemters = new List<DbParameter>();
            var isTracked = _tracker.IsTracked(entry.Entity);

            foreach (var item in entry.Members)
            {
                if (item.IsIncrement || item.IsPrimary || entry.Primary == item)
                {
                    continue;
                }

                var parameter = new DbParameter(GetNewParameterName(), item.GetValue(entry.Entity));
                if (isTracked && !_tracker.IsValueChanged(entry.Entity, item.Member, parameter.Value))
                {
                    continue;
                }

                columns.Add(GetColumnName(item));
                paramemters.Add(parameter);
            }

            AddCommandToGroup(
                new DbUpdateCommand(
                    entry,
                    columns,
                    paramemters,
                    GetColumnName(entry.Primary),
                    GetDbParameter(entry.Entity, entry.Primary)),
                flushMode
            );
        }

        protected void AddDeleteEntry(EntityEntry entry, FlushMode flushMode)
        {
            AddCommandToGroup(
                new DbDeleteCommand(
                   entry,
                   GetColumnName(entry.Primary),
                   new List<DbParameter>() { GetDbParameter(entry.Entity, entry.Primary) }),
                flushMode
              );
        }

        protected void AddCommandToGroup(DbModifyCommand modifyCommand, FlushMode flushMode)
        {
            var group = FindCommadGroup(modifyCommand, flushMode);
            if (modifyCommand.Is<DbUpdateCommand>())
            {
                group.Commands.Add(modifyCommand);
                return;
            }

            var last = group.Commands.LastOrDefault();
            if (last == null ||
                last.Entity.State != modifyCommand.Entity.State ||
                last.Entity.Type != modifyCommand.Entity.Type)
            {
                group.Commands.Add(modifyCommand);
                return;
            }

            if (modifyCommand.Is<DbInsertCommand>())
            {
                last.DbParams.AddRange(modifyCommand.DbParams);
            }
            else
            {
                //modifyCommand.PrimaryKeyValue
            }
        }

        protected DbModifyCommandGroup FindCommadGroup(DbModifyCommand modifyCommand, FlushMode flushMode)
        {
            var group = _groups.LastOrDefault();
            if (group != null && group.DbParameterCount + modifyCommand.DbParameterCount < MaxParameterCount)
            {
                if (flushMode == FlushMode.Default)
                {
                    return group;
                }

                if ((modifyCommand.Entity.State != EntityState.Insert || modifyCommand.Entity.Increment == null) &&
                    group.Commands.Any(item => item.Entity.State != EntityState.Insert || item.Entity.Increment == null))
                {
                    return group;
                }
            }

            _groups.Add(new DbModifyCommandGroup());
            return _groups.LastOrDefault();
        }

        protected string GetNewParameterName()
        {
            return "@P" + (_colPostfix++).ToString();
        }

        protected string GetColumnName(MemberDescriptor memberDescriptor)
        {
            return memberDescriptor.Member.GetCustomAttribute<ColumnAttribute>()?.Name ?? memberDescriptor.Member.Name;
        }

        protected DbParameter GetDbParameter(object entity, MemberDescriptor memberDescriptor)
        {
            return new DbParameter(GetNewParameterName(), memberDescriptor.GetValue(entity));
        }
    }

    public enum FlushMode
    {
        Default = 0,

        AutoGetIncrement
    }
}
