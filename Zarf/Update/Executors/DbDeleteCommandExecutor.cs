using Zarf.Builders;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Update.Commands;

namespace Zarf.Update.Executors
{
    public class DbDeleteCommandExecutor : DbCommandExecutor<DbDeleteCommand>
    {
        public DbDeleteCommandExecutor(IDataBaseFacade dataBase, ISqlTextBuilder sqlBuilder, IModifyOperationCompiler compiler)
            : base(dataBase, sqlBuilder, compiler)
        {

        }

        public override int ExecuteCore(string commandText, DbDeleteCommand modifyCommand)
        {
            return DataBase.GetCommand().ExecuteScalar<int>(commandText, modifyCommand.IdentityColumnValue);
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
