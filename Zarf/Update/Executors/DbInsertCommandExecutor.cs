using System.Linq;
using Zarf.Builders;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Update.Commands;

namespace Zarf.Update.Executors
{
    public class DbInsertCommandExecutor : DbCommandExecutor<DbInsertCommand>
    {
        public DbInsertCommandExecutor(IDbCommandFacotry commandFacotry, ISqlTextBuilder sqlBuilder, IModifyOperationCompiler compiler)
            : base(commandFacotry, sqlBuilder, compiler)
        {

        }

        public override string GetCommandText(DbInsertCommand modifyCommand)
        {
            var insertExpression = new InsertExpression(
                modifyCommand.Entity.Type.ToTable(),
                modifyCommand.DbParams,
                modifyCommand.Columns,
                modifyCommand.Entity.Increment?.Member);

            return SqlBuilder.Build(insertExpression);
        }

        public override int ExecuteCore(string commandText, DbInsertCommand modifyCommand)
        {
            var autoIncrementValue = CommandFacotry.Create().ExecuteScalar<int>(commandText, modifyCommand.DbParams.ToArray());
            if (modifyCommand.Entity.Increment != null)
            {
                modifyCommand.Entity.Increment.SetValue(modifyCommand.Entity.Entity, autoIncrementValue);
            }

            return autoIncrementValue;
        }
    }
}
