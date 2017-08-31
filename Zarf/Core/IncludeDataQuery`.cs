using System.Linq.Expressions;
using System.Linq;

namespace Zarf
{
    public class IncludeDataQuery<TEntity, TProperty> : DataQuery<TEntity>, IIncludeDataQuery<TEntity, TProperty>
    {
        public IncludeDataQuery(IQueryProvider provider)
            : base(provider)
        {

        }

        public IncludeDataQuery(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {

        }
    }
}
