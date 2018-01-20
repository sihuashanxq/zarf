using System.Collections.Generic;
using System.Linq;
using Zarf.Entities;
using Zarf.Extensions;

namespace Zarf.Update
{
    //Entry中包含列
    public class DbModificationCommand
    {
        public Table Table { get; }

        public EntityEntry Entry { get; }

        public List<string> Columns { get; }

        public List<DbParameter> Parameters { get; }

        public string PrimaryKey { get; }

        public List<DbParameter> PrimaryKeyValues { get; }

        public int ParameterCount => (Parameters?.Count ?? 0) + (PrimaryKeyValues?.Count ?? 0);

        public EntityState State => Entry.State;

        public DbModificationCommand(EntityEntry entity)
        {
            Entry = entity;
            Table = Entry.Type.ToTable();
        }

        public DbModificationCommand(EntityEntry entity, IEnumerable<string> columns, IEnumerable<DbParameter> columnParams)
            : this(entity)
        {
            Columns = columns.ToList();
            Parameters = columnParams.ToList();
        }

        public DbModificationCommand(EntityEntry entry, IEnumerable<string> columns, IEnumerable<DbParameter> columnParams, string primaryKey, DbParameter primaryKeyValue)
            : this(entry, columns, columnParams)
        {
            PrimaryKey = primaryKey;
            PrimaryKeyValues = new List<DbParameter>() { primaryKeyValue };
        }

        public DbModificationCommand(EntityEntry entity, string primaryKey, List<DbParameter> primaryKeyValues)
            : this(entity)
        {
            PrimaryKey = primaryKey;
            PrimaryKeyValues = primaryKeyValues;
        }
    }
}
