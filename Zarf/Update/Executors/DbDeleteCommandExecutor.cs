using Zarf.Builders;
using Zarf.Core;
using Zarf.Query.Expressions;
using Zarf.Update.Commands;

namespace Zarf.Update.Executors
{
    public class DbDeleteCommandExecutor : DbCommandExecutor<DbDeleteCommand>
    {
        public DbDeleteCommandExecutor(IDbCommandFacotry commandFacotry, ISqlTextBuilder sqlBuilder, IModifyOperationCompiler compiler)
            : base(commandFacotry, sqlBuilder, compiler)
        {

        }

        public override int ExecuteCore(string commandText, DbDeleteCommand modifyCommand)
        {
            return CommandFacotry.Create().ExecuteScalar<int>(commandText, modifyCommand.IdentityColumnValue);
        }

        public override string GetCommandText(DbDeleteCommand modifyCommand)
        {
            var deleteExpression = new DeleteExpression(
                modifyCommand.Table,
                modifyCommand.IdentityColumn,
                modifyCommand.IdentityColumnValue);

            return SqlBuilder.Build(deleteExpression);
        }
    }
}
