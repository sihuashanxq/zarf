using System.Linq;
using System.Linq.Expressions;
using Zarf.Query;
using Zarf.Core;
using Zarf.Core.Internals;

namespace Zarf
{
    public class QueryProvider : IQueryProvider
    {
        public DbContext Context { get; }

        public IQueryExecutor QueryInterpreter { get; }

        public QueryProvider(DbContext dbContext)
        {
            Context = dbContext;
            QueryInterpreter = new QueryExecutor(dbContext.DbContextParts);
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
            return QueryInterpreter.ExecuteSingle<TResult>(query);
        }
    }
}
