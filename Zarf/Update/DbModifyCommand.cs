using System.Collections.Generic;
using System.Linq;
using Zarf.Entities;
using Zarf.Extensions;

namespace Zarf.Update.Commands
{
    public class DbModifyCommand
    {
        public Table Table { get; }

        public EntityEntry Entry { get; }

        public List<string> Columns { get; protected set; }

        public List<DbParameter> DbParams { get; protected set; }

        public string PrimaryKey { get; protected set; }

        public List<DbParameter> PrimaryKeyValues { get; protected set; }

        public int DbParameterCount => (DbParams?.Count ?? 0) + (PrimaryKeyValues?.Count ?? 0);

        public EntityState State => Entry.State;

        public DbModifyCommand(EntityEntry entity)
        {
            Entry = entity;
            Table = Entry.Type.ToTable();
        }

        public DbModifyCommand(
            EntityEntry entity,
            IEnumerable<string> columns,
            IEnumerable<DbParameter> dbParams)
            : this(entity)
        {
            Columns = columns.ToList();
            DbParams = dbParams.ToList();
        }
    }

    public class DbModifyCommandGroup
    {
        public List<DbModifyCommand> Commands { get; }

        public int DbParameterCount
        {
            get
            {
                return Commands?.Sum(item => item.DbParameterCount) ?? 0;
            }
        }

        public DbModifyCommandGroup()
        {
            Commands = new List<DbModifyCommand>();
        }

        public IEnumerable<DbParameter> Parameters
        {
            get
            {
                var parameters = new List<DbParameter>();
                foreach (var item in Commands)
                {
                    if (item.DbParams != null)
                    {
                        parameters.AddRange(item.DbParams);
                    }

                    if (item.PrimaryKeyValues != null)
                    {
                        parameters.AddRange(item.PrimaryKeyValues);
                    }
                }

                return parameters;
            }
        }
    }
}
