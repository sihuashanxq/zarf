using System;
using System.Collections.Generic;
using System.Linq;
using Zarf.Builders;
using Zarf.Core;
using Zarf.Update.Expressions;

namespace Zarf.Update.Executors
{
    public class DbModifyExecutor : IDbModifyExecutor
    {
        public IDbEntityCommandFacotry CommandFacotry { get; }

        public ISqlTextBuilder SqlBuilder { get; }

        public DbModificationCommandGroupBuilder CommandGroupBuilder { get; }

        public DbModifyExecutor(IDbEntityCommandFacotry commandFacotry, ISqlTextBuilder sqlBuilder, IEntityTracker tracker)
        {
            CommandFacotry = commandFacotry;
            SqlBuilder = sqlBuilder;
            CommandGroupBuilder = new DbModificationCommandGroupBuilder(tracker);
        }

        public virtual int Execute(IEnumerable<EntityEntry> entries)
        {
            var commandGroups = CommandGroupBuilder.Build(entries);
            var dbCommand = CommandFacotry.Create();
            var modifyRowcount = 0;

            foreach (var commandGroup in commandGroups)
            {
                if (commandGroup.Commands.Count == 1)
                {
                    var modifyCommand = commandGroup.Commands.FirstOrDefault();
                    if (modifyCommand.Entry.State == EntityState.Insert && modifyCommand.Entry.AutoIncrementProperty != null)
                    {
                        using (var reader = dbCommand.ExecuteDataReader(BuildCommandText(commandGroup), commandGroup.Parameters.ToArray()))
                        {
                            if (!reader.Read())
                            {
                                throw new Exception("insert data error!");
                            }
                            modifyCommand.Entry.AutoIncrementProperty.SetValue(modifyCommand.Entry.Entity, reader[0]);
                            modifyRowcount += reader.GetInt32(1);
                            continue;
                        }
                    }
                }

                modifyRowcount += dbCommand.ExecuteScalar<int>(BuildCommandText(commandGroup), commandGroup.Parameters.ToArray());
            }

            return modifyRowcount;
        }

        public string BuildCommandText(DbModificationCommandGroup commandGroup)
        {
            return SqlBuilder.Build(DbStoreExpressionFacotry.Default.Create(commandGroup));
        }
    }
}
