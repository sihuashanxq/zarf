using Zarf.Update.Commands;
using Zarf.Core;
using Zarf.Builders;
using System.Linq;
using Zarf.Update.Compilers;
using System.Collections.Generic;

namespace Zarf.Update.Executors
{
    public class CompositeDbCommandExecutor : IDbCommandExecutor
    {
        protected IDbCommandExecutor<DbInsertCommand> InsertExecutor { get; }

        protected IDbCommandExecutor<DbUpdateCommand> UpdateExecutor { get; }

        protected IDbCommandExecutor<DbDeleteCommand> DeleteExecutor { get; }

        public CompositeDbCommandExecutor(IDbCommandFacotry commandFacotry, ISqlTextBuilder sqlBuilder, IEntityTracker tracker)
        {
            var compiler = new CompositeModifyOperationCompiler(tracker);
            InsertExecutor = new DbInsertCommandExecutor(commandFacotry, sqlBuilder, compiler);
            UpdateExecutor = new DbUpdateCommandExecutor(commandFacotry, sqlBuilder, compiler);
            DeleteExecutor = new DbDeleteCommandExecutor(commandFacotry, sqlBuilder, compiler);
        }

        public int Execute(IEnumerable<EntityEntry> entries)
        {
            var inserts = entries.Where(item => item.State == EntityState.Insert).GroupBy(item => item.Type);
            var updates = entries.Where(item => item.State == EntityState.Update).GroupBy(item => item.Type);
            var deletes = entries.Where(item => item.State == EntityState.Delete).GroupBy(item => item.Type);
            //FlushMode==AutoSetIncrement

            //合并
            foreach (var item in entries.OrderBy(item => item.State).ThenByDescending(item => item.Entity.GetType().GetHashCode()))
            {
                switch (item.State)
                {
                    case EntityState.Insert:
                        InsertExecutor.Execute(new[] { item });
                        break;
                    case EntityState.Update:
                        UpdateExecutor.Execute(new[] { item });
                        break;
                    default:
                        DeleteExecutor.Execute(new[] { item });
                        break;
                }
            }

            return 0;
        }
    }
}
