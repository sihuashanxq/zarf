using System.Collections.Generic;
using System.Linq;
using Zarf.Builders;
using Zarf.Core;
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

        public virtual int Execute(IEnumerable<EntityEntry> entries)
        {
            var commands = GetModifyCommand(entries);
            foreach (var command in commands)
            {
                var commandText = GetCommandText(command);
                ExecuteCore(commandText, command);
            }

            return 0;
        }

        public abstract int ExecuteCore(string commandText, TModifyCommand modifyCommand);

        public abstract string GetCommandText(TModifyCommand modifyCommand);

        protected virtual IEnumerable<TModifyCommand> GetModifyCommand(IEnumerable<EntityEntry> entries)
        {
            return Compiler.Compile(entries).OfType<TModifyCommand>();
        }
    }
}
