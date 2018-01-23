using System;
using System.Collections.Generic;

namespace Zarf.Query.Internals.ModelTypes
{
    /// <summary>
    /// QueryModelType Cahce
    /// </summary>
    internal class QueryModelTypeCache
    {
        public Type ModelType { get; set; }

        public Dictionary<string, Type> Fields { get; set; }
    }
}
