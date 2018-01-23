using System.Linq;
using System.Linq.Expressions;
using Zarf.Core.Internals;
using Zarf.Query;

namespace Zarf
{
    public class QueryProvider : IQueryProvider
    {
        public DbContext Context { get; }

        public IQueryExecutor Executor { get; }

        public QueryProvider(DbContext context, IQueryExecutor executor)
        {
            Context = context;
            Executor = executor;
        }

        public IQueryable CreateQuery(Expression query)
        {
            return CreateQuery<object>(query);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression query)
        {
            return new InternalQuery<TElement>(this, query);
        }

        public object Execute(Expression query)
        {
            return Execute<object>(query);
        }

        public TResult Execute<TResult>(Expression query)
        {
            return Executor.ExecuteSingle<TResult>(query, Context.QueryContextFactory.CreateContext());
        }
    }
}
