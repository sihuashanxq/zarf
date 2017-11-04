using System;
using Zarf.Update.Commands;
using System.Data;
using Zarf.Builders;
using Zarf.Entities;
using Zarf.Core;

namespace Zarf.Update.Executors
{
    public abstract class DbCommandExecutor<TCommand> : IDbCommandExecutor<TCommand>
        where TCommand : DbModifyCommand
    {
        public IDataBaseFacade DataBase { get; }

        public ISqlTextBuilder SqlBuilder { get; }

        public DbCommandExecutor(IDataBaseFacade dataBase, ISqlTextBuilder sqlBuilder)
        {
            DataBase = dataBase;
            SqlBuilder = sqlBuilder;
        }

        public virtual int Execute(TCommand modifyCommand)
        {
            return ExecuteCore(GetCommandText(modifyCommand), modifyCommand);
        }

        public abstract int ExecuteCore(string commandText, TCommand modifyCommand);

        public abstract string GetCommandText(TCommand modifyCommand);
    }
}
