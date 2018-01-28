using System;
using System.Collections.Generic;
using System.Text;
using Zarf.Generators.Functions;

namespace Zarf.Metadata.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SQLFunctionHandlerAttribute : Attribute
    {
        public Type Handler { get; set; }

        public SQLFunctionHandlerAttribute(Type handler)
        {
            if (!typeof(ISQLFunctionHandler).IsAssignableFrom(handler))
            {
                throw new NotSupportedException($"handler type:{handler.Name} not implemented ISQLFunctionHandler interface!");
            }

            Handler = handler;
        }
    }
}
