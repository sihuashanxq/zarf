using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

using Zarf.Queries.Expressions;

namespace Zarf.Queries
{
    /// <summary>
    /// 属性导航
    /// </summary>
    public class PropertyNavigation
    {
        public MemberInfo Property { get; }

        public QueryExpression RefrenceQuery { get; }

        public List<Expression> RefrenceColumns { get; }

        public Expression Relation { get; }

        public PropertyNavigation(
            MemberInfo property,
            QueryExpression refrenceQuery,
            List<Expression> refrenceColumns,
            Expression relation)
        {
            Property = property;
            RefrenceQuery = refrenceQuery;
            RefrenceColumns = refrenceColumns;
            Relation = relation;
        }
    }
}
