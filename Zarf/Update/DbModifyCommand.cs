using System.Collections.Generic;
using System.Linq;
using Zarf.Entities;
using Zarf.Extensions;

namespace Zarf.Update.Commands
{
    public class DbModifyCommand
    {
        public Table Table { get; }

        public EntityEntry Entity { get; }

        public List<string> Columns { get; protected set; }

        public List<DbParameter> DbParams { get; protected set; }

        public string PrimaryKey { get; protected set; }

        public List<DbParameter> PrimaryKeyValues { get; protected set; }

        public int DbParameterCount => (DbParams?.Count ?? 0) + (PrimaryKeyValues?.Count ?? 0);

        public DbModifyCommand(EntityEntry entity)
        {
            Entity = entity;
            Table = Entity.Type.ToTable();
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
    }
}
