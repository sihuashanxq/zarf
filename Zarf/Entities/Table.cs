namespace Zarf.Entities
{
    /// <summary>
    /// 数据表
    /// </summary>
    public class Table
    {
        /// <summary>
        /// 数据表名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 数据表架构
        /// </summary>
        public string Schema { get; }

        public Table(string tableName, string schema = "dbo")
        {
            Name = tableName;
            Schema = schema;
        }
    }
}
