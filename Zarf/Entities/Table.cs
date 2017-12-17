using System;

namespace Zarf.Entities
{
    public class Table
    {
        public string Name { get; }

        public string Schema { get; }

        public Table(string tableName, string schema = "dbo")
        {
            Name = tableName;
            Schema = schema;
        }
    }
}
