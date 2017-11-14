using System.Linq;
using Zarf.Builders;
using Zarf.Core;
using Zarf.Query.Expressions;
using Zarf.Update.Commands;

namespace Zarf.Update.Executors
{
    public class DbUpdateCommandExecutor : DbCommandExecutor<DbUpdateCommand>
    {
        public DbUpdateCommandExecutor(IDbCommandFacotry commandFacotry, ISqlTextBuilder sqlBuilder, IModifyOperationCompiler compiler)
            : base(commandFacotry, sqlBuilder, compiler)
        {

        }

        public override int ExecuteCore(string commandText, DbUpdateCommand modifyCommand)
        {
            if (modifyCommand.Columns == null || modifyCommand.Columns.Count() == 0)
            {
                return 0;
            }

            return
                CommandFacotry
                .Create()
                .ExecuteScalar<int>(
                    commandText,
                    modifyCommand
                        .DbParams
                        .ToList()
                        .Concat(modifyCommand.PrimaryKeyValues)
                        .ToArray()
                );
        }

        public override string GetCommandText(DbUpdateCommand modifyCommand)
        {
            var updateExpression = new UpdateExpression(
                modifyCommand.Table,
                modifyCommand.DbParams,
                modifyCommand.Columns,
                modifyCommand.PrimaryKey,
                modifyCommand.PrimaryKeyValues.FirstOrDefault());

            return SqlBuilder.Build(updateExpression);
        }
    }
}
