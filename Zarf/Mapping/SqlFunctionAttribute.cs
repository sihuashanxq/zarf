using System;

namespace Zarf.Mapping
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SqlFunctionAttribute : Attribute
    {
        public string Schema { get; set; } = "dbo";

        public string Name { get; set; }
    }
}
