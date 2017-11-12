using Zarf.Update.Commands;
using Zarf.Core;
using Zarf.Builders;
using System.Linq;
using Zarf.Update.Compilers;

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

        public int Execute(DbModifyOperation modifyOperation)
        {
            switch (modifyOperation.Entries.FirstOrDefault().State)
            {
                case EntityState.Insert:
                    return InsertExecutor.Execute(modifyOperation);
                case EntityState.Update:
                    return UpdateExecutor.Execute(modifyOperation);
                default:
                    return DeleteExecutor.Execute(modifyOperation);
            }
        }
    }
}
