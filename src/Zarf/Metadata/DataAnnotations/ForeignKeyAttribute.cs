using System;

namespace Zarf.Metadata.DataAnnotations
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
