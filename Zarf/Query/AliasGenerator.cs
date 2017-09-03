using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query
{
    /// <summary>
    /// 别名生成器
    /// </summary>
    public class AliasGenerator : IAliasGenerator
    {
        private int _tableRefCount;

        private int _columnRefCount;

        public string GetNewTableAlias()
        {
            return "T" + _tableRefCount++;
        }

        public string GetNewColumnAlias()
        {
            return "C" + _tableRefCount++;
        }

        /// <summary>
        /// 不重置表别名
        /// </summary>
        public void Reset()
        {
            _columnRefCount = 0;
        }
    }
}
