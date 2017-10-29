using System.Linq;
using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf
{
    public class DbQueryProvider : IQueryProvider
    {
        private IQueryInterpreter _queryInterpreter;

        public DbQueryProvider()
        {
            _queryInterpreter = new QueryInterpreter();
        }

        public IQueryable CreateQuery(Expression query)
        {
            return CreateQuery<object>(query);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression query)
        {
            return new DbQuery<TElement>(this, query);
        }

        public object Execute(Expression query)
        {
            return Execute<object>(query);
        }

        public TResult Execute<TResult>(Expression query)
        {
            return _queryInterpreter.ExecuteSingle<TResult>(query);
        }
    }
}
