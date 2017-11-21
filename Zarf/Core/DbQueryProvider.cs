using System.Linq;
using System.Linq.Expressions;
using Zarf.Query;
using Zarf.Core;

namespace Zarf
{
    public class DbQueryProvider : IQueryProvider
    {
        public DbContext Context { get; }

        public IQueryInterpreter QueryInterpreter { get; }

        public DbQueryProvider(DbContext dbContext)
        {
            Context = dbContext;
            QueryInterpreter = new QueryInterpreter(dbContext.DbContextParts);
        }

        public IQueryable CreateQuery(Expression query)
        {
            return CreateQuery<object>(query);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression query)
        {
            return new InternalDbQuery<TElement>(this, query);
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
