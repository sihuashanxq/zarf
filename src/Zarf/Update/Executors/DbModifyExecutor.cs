using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zarf.Generators;
using Zarf.Core;
using Zarf.Update.Expressions;
using Zarf.Metadata.Entities;

namespace Zarf.Update.Executors
{
    public class DbModifyExecutor : IDbModifyExecutor
    {
        public IDbEntityCommandFacotry CommandFacotry { get; }

        public IDbEntityConnectionFacotry ConnectionFacotry { get; }

        public ISQLGenerator SQLGenerator { get; }

        public IDbModificationCommandGroupBuilder CommandGroupBuilder { get; }

        public DbModifyExecutor(IDbEntityCommandFacotry commandFacotry, IDbEntityConnectionFacotry connectionFacotry, ISQLGenerator sqlBuilder, IDbModificationCommandGroupBuilder groupBuilder)
        {
            CommandFacotry = commandFacotry;
            ConnectionFacotry = connectionFacotry;
            SQLGenerator = sqlBuilder;
            CommandGroupBuilder = groupBuilder;
        }

        public virtual int Execute(IEnumerable<EntityEntry> entries)
        {
            return ExecuteAsync(entries).GetAwaiter().GetResult();
        }

        public async Task<int> ExecuteAsync(IEnumerable<EntityEntry> entries)
        {
            var commandGroups = CommandGroupBuilder.Build(entries);
            var dbCommand = CommandFacotry.Create(ConnectionFacotry.CreateDbContextScopedConnection());
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
            return SQLGenerator.Generate(DbStoreExpressionFacotry.Default.Create(commandGroup), new List<DbParameter>());
        }
    }
}
