using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zarf.Generators;
using Zarf.Core;
using Zarf.Update.Expressions;
using Zarf.Entities;

namespace Zarf.Update.Executors
{
    public class DbModifyExecutor : IDbModifyExecutor
    {
        public IDbEntityCommandFacotry CommandFacotry { get; }

        public ISQLGenerator SQLGenerator { get; }

        public DbModificationCommandGroupBuilder CommandGroupBuilder { get; }

        public DbModifyExecutor(IDbEntityCommandFacotry commandFacotry, ISQLGenerator sqlBuilder, IEntityTracker tracker)
        {
            CommandFacotry = commandFacotry;
            SQLGenerator = sqlBuilder;
            CommandGroupBuilder = new DbModificationCommandGroupBuilder(tracker);
        }

        public virtual int Execute(IEnumerable<EntityEntry> entries)
        {
            return ExecuteAsync(entries).GetAwaiter().GetResult();
        }

        public async Task<int> ExecuteAsync(IEnumerable<EntityEntry> entries)
        {
            var commandGroups = CommandGroupBuilder.Build(entries);
            var dbCommand = CommandFacotry.Create();
            var modifyRowcount = 0;

            foreach (var commandGroup in commandGroups)
            {
                using (var reader = await dbCommand.ExecuteDataReaderAsync(BuildCommandText(commandGroup), commandGroup.Parameters.ToArray()))
                {
                    if (!reader.Read())
                    {
                        throw new Exception("write data error!");
                    }

                    var modifyCommand = commandGroup.Commands.FirstOrDefault();
                    if (commandGroup.Commands.Count == 1 &&
                        modifyCommand.Entry.State == EntityState.Insert &&
                        modifyCommand.Entry.AutoIncrementProperty != null)
                    {
                        modifyCommand.Entry.AutoIncrementProperty.SetValue(modifyCommand.Entry.Entity, reader[1]);
                    }

                    modifyRowcount += reader.GetInt32(0);
                }
            }

            return modifyRowcount;
        }

        public string BuildCommandText(DbModificationCommandGroup commandGroup)
        {
            return SQLGenerator.Generate(DbStoreExpressionFacotry.Default.Create(commandGroup));
        }
    }
}
