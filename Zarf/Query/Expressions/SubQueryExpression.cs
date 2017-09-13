using System;

namespace Zarf.Query.Expressions
{
    /// <summary>
    /// 子查询
    /// </summary>
    public class SubQueryExpression : QueryExpression
    {
        public SubQueryExpression(Type entityType, string alias)
            : base(entityType, alias)
        {
            //TODO
        }
    }
}
