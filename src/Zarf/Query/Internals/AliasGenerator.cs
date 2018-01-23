namespace Zarf.Query.Internals
{
    /// <summary>
    /// 别名生成器
    /// </summary>
    public class AliasGenerator : IAliasGenerator
    {
        private int _tableCount;

        private int _columnCount;

        private int _parameterCount;

        public string GetNewTable()
        {
            return "T" + _tableCount++;
        }

        public string GetNewColumn()
        {
            return "C" + _columnCount++;
        }

        public string GetNewParameter()
        {
            return "P" + _parameterCount++;
        }

        /// <summary>
        /// 不重置表别名
        /// </summary>
        public void Reset()
        {
            _columnCount = 0;
        }
    }
}
