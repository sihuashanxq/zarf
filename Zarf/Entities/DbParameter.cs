using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Entities
{
    public class DbParameter
    {
        public object Value { get; }

        public string Name { get; }

        public DbParameter(string name, object value)
        {
            Value = value;
            Name = name;
        }
    }
}
