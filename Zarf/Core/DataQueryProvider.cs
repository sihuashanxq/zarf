using System.Linq;
using System.Linq.Expressions;

namespace Zarf
{
    public class DataQueryProvider : IQueryProvider
    {
        public IQueryable CreateQuery(Expression node)
        {
            return CreateQuery<object>(node);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression node)
        {
            return new DataQuery<TElement>(this, node);
        }

        public object Execute(Expression linqExpression)
        {
            return Execute<object>(linqExpression);
        }

        public TResult Execute<TResult>(Expression linqExpression)
        {
            return new LinqExpressionInvoker().Invoke<TResult>(linqExpression);
        }
    }
}
