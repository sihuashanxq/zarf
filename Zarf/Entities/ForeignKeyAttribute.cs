using System;

namespace Zarf.Entities
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ForeignKeyAttribute : Attribute
    {
        public string Name { get; set; }

        public ForeignKeyAttribute(string name)
        {
            Name = name;
        }
    }
}
