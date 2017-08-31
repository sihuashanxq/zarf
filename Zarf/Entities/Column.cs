using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Entities
{
    /// <summary>
    /// 数据列
    /// </summary>
    public class Column
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; }

        public Column(string name)
        {
            Name = name;
        }
    }
}
