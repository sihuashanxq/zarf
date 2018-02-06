using System;
using System.IO;

namespace Zarf.Metadata.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public string Name { get; }

        public string Schema { get; set; }

        public TableAttribute(string name)
        {
            Name = name;
        }
    }
}
