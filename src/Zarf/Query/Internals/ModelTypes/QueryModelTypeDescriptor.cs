using System;
using System.Collections.Generic;
using Zarf.Query.Expressions;

namespace Zarf.Query.Internals.ModelTypes
{
    /// <summary>
    /// QueryModelType 描述符
    /// </summary>
    internal class QueryModelTypeDescriptor
    {
        /// <summary>
        /// 引用的外部列与字段之间的映射关系
        /// </summary>
        public Dictionary<string, ColumnExpression> FieldMaps { get; set; }

        public Type SubModelType { get; set; }
    }
}
