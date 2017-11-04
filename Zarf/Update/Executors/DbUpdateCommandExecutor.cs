using System.Linq;
using Zarf.Builders;
using Zarf.Core;
using Zarf.Query.Expressions;
using Zarf.Update.Commands;

namespace Zarf.Update.Executors
{
    public class DbUpdateCommandExecutor : DbCommandExecutor<DbUpdateCommand>
    {
        public DbUpdateCommandExecutor(IDataBaseFacade dataBase, ISqlTextBuilder sqlBuilder)
            : base(dataBase, sqlBuilder)
        {

        }

        public override int ExecuteCore(string commandText, DbUpdateCommand modifyCommand)
        {
            var dbCommand = DataBase.GetCommand();
            var dbParams = modifyCommand.DbParams.ToList().Concat(new[] { modifyCommand.IdentityColumnValue });
            return dbCommand.ExecuteScalar<int>(commandText, dbParams.ToArray());
        }

        public override string GetCommandText(DbUpdateCommand modifyCommand)
        {
            var updateExpression = new UpdateExpression(
                modifyCommand.Table,
                modifyCommand.DbParams,
                modifyCommand.Columns,
                modifyCommand.IdentityColumn,
                modifyCommand.IdentityColumnValue);

            return SqlBuilder.Build(updateExpression);
        }
    }
}
