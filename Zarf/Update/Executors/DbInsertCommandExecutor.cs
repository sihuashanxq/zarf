using Zarf.Builders;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Update.Commands;
using System.Linq;
using Zarf.Core;

namespace Zarf.Update.Executors
{
    public class DbInsertCommandExecutor : DbCommandExecutor<DbInsertCommand>
    {
        public DbInsertCommandExecutor(IDataBaseFacade dataBase, ISqlTextBuilder sqlBuilder)
            : base(dataBase, sqlBuilder)
        {

        }

        public override string GetCommandText(DbInsertCommand modifyCommand)
        {
            var insertExpression = new InsertExpression(
                modifyCommand.Entity.Type.ToTable(),
                modifyCommand.DbParams,
                modifyCommand.Columns,
                modifyCommand.Increment?.Member);

            return SqlBuilder.Build(insertExpression);
        }

        public override int ExecuteCore(string commandText, DbInsertCommand modifyCommand)
        {
            var dbCommand = DataBase.GetCommand();
            var autoIncrementValue = dbCommand.ExecuteScalar<int>(commandText, modifyCommand.DbParams.ToArray());

            if (modifyCommand.Entity.Increment != null)
            {
                modifyCommand.Entity.Increment.SetValue(modifyCommand.Entity.Entity, autoIncrementValue);
            }

            return autoIncrementValue;
        }
    }
}
