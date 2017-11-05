using System;
using Zarf.Update.Commands;
using Zarf.Core;
using Zarf.Builders;
using System.Linq;

namespace Zarf.Update.Executors
{
    public class CompisteDbCommandExecutor : IDbCommandExecutor
    {
        protected IDbCommandExecutor<DbInsertCommand> InsertExecutor { get; }

        protected IDbCommandExecutor<DbUpdateCommand> UpdateExecutor { get; }

        protected IDbCommandExecutor<DbDeleteCommand> DeleteExecutor { get; }

        public CompisteDbCommandExecutor(IDataBaseFacade dataBase, ISqlTextBuilder sqlBuilder, IModifyOperationCompiler compiler)
        {
            InsertExecutor = new DbInsertCommandExecutor(dataBase, sqlBuilder, compiler);
            UpdateExecutor = new DbUpdateCommandExecutor(dataBase, sqlBuilder, compiler);
            DeleteExecutor = new DbDeleteCommandExecutor(dataBase, sqlBuilder, compiler);
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
