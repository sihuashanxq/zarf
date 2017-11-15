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

        public List<string> Columns { get; }

        public List<DbParameter> DbParams { get; }

        public string PrimaryKey { get; }

        public List<DbParameter> PrimaryKeyValues { get; }

        public int DbParameterCount => (DbParams?.Count ?? 0) + (PrimaryKeyValues?.Count ?? 0);

        public EntityState State => Entry.State;

        public DbModifyCommand(EntityEntry entity)
        {
            Entry = entity;
            Table = Entry.Type.ToTable();
        }

        public DbModifyCommand(EntityEntry entity, IEnumerable<string> columns, IEnumerable<DbParameter> columnParams)
            : this(entity)
        {
            Columns = columns.ToList();
            DbParams = columnParams.ToList();
        }

        public DbModifyCommand(EntityEntry entry, IEnumerable<string> columns, IEnumerable<DbParameter> columnParams, string primaryKey, DbParameter primaryKeyValue)
            : this(entry, columns, columnParams)
        {
            PrimaryKey = primaryKey;
            PrimaryKeyValues = new List<DbParameter>() { primaryKeyValue };
        }

        public DbModifyCommand(EntityEntry entity, string primaryKey, List<DbParameter> primaryKeyValues)
            : this(entity)
        {
            PrimaryKey = primaryKey;
            PrimaryKeyValues = primaryKeyValues;
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
