using Zarf.Update.Commands;
using Zarf.Core;
using Zarf.Builders;
using System.Linq;
using Zarf.Update.Compilers;

namespace Zarf.Update.Executors
{
    public class CompisteDbCommandExecutor : IDbCommandExecutor
    {
        protected IDbCommandExecutor<DbInsertCommand> InsertExecutor { get; }

        protected IDbCommandExecutor<DbUpdateCommand> UpdateExecutor { get; }

        protected IDbCommandExecutor<DbDeleteCommand> DeleteExecutor { get; }

        protected IModifyOperationCompiler Compiler { get; }

        public CompisteDbCommandExecutor(IDbCommandFacotry commandFacotry, ISqlTextBuilder sqlBuilder)
        {
            Compiler = new CompositeModifyOperationCompiler();
            InsertExecutor = new DbInsertCommandExecutor(commandFacotry, sqlBuilder, Compiler);
            UpdateExecutor = new DbUpdateCommandExecutor(commandFacotry, sqlBuilder, Compiler);
            DeleteExecutor = new DbDeleteCommandExecutor(commandFacotry, sqlBuilder, Compiler);
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
