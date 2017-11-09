using System;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Builders;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Update.Commands;

namespace Zarf.Update.Executors
{
    public abstract class DbCommandExecutor<TModifyCommand> : IDbCommandExecutor<TModifyCommand>
        where TModifyCommand : DbModifyCommand
    {
        public IDbCommandFacotry CommandFacotry { get; }

        public ISqlTextBuilder SqlBuilder { get; }

        public IModifyOperationCompiler Compiler { get; }

        public DbCommandExecutor(IDbCommandFacotry commandFacotry, ISqlTextBuilder sqlBuilder, IModifyOperationCompiler compiler)
        {
            CommandFacotry = commandFacotry;
            SqlBuilder = sqlBuilder;
            Compiler = compiler;
        }

        public virtual int Execute(DbModifyOperation modifyOperation)
        {
            var modifyCommand = GetModifyCommand(modifyOperation);
            var commandText = GetCommandText(modifyCommand);

            return ExecuteCore(commandText, modifyCommand);
        }

        public abstract int ExecuteCore(string commandText, TModifyCommand modifyCommand);

        public abstract string GetCommandText(TModifyCommand modifyCommand);

        protected virtual TModifyCommand GetModifyCommand(DbModifyOperation modifyOperation)
        {
            return Compiler.Compile(modifyOperation).FirstOrDefault().As<TModifyCommand>();
        }
    }
}
