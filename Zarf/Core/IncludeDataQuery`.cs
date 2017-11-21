using System.Linq.Expressions;
using System.Linq;
using Zarf.Core;
namespace Zarf
{
    public class IncludeDataQuery<TEntity, TProperty> : InternalDbQuery<TEntity>, IIncludeDataQuery<TEntity, TProperty>
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
