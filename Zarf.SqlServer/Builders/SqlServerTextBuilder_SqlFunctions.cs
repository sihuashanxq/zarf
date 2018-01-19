using System;
using System.Linq.Expressions;
using Zarf.Queries.Expressions;
using Zarf.Builders;

namespace Zarf.SqlServer.Builders
{
    internal partial class SqlServerTextBuilder : SqlTextBuilder
    {
        protected static readonly Type StringType = typeof(string);

        protected static readonly Type DateType = typeof(DateTime);

        protected static readonly Type MathType = typeof(Math);

        /// <summary>
        /// Sql 函数
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        protected override Expression VisitSqlFunction(SqlFunctionExpression function)
        {
            return function;
        }
    }
}
