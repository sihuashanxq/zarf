using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Query
{
    public class QuerySourceProvider : IQuerySourceProvider
    {
        protected virtual Dictionary<Expression, Expression> Sources { get; set; }

        public QuerySourceProvider()
        {
            Sources = new Dictionary<Expression, Expression>();
        }

        public void AddSource(Expression expression, Expression source)
        {
            Sources[expression] = source;
        }

        public Expression GetSource(Expression expression)
        {
            if (Sources.TryGetValue(expression, out Expression source))
            {
                return source;
            }

            return null;
        }
    }
}
