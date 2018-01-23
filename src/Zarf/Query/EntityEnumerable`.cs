using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Query
{
    public class EntityEnumerable<TEntity> : IEnumerable<TEntity>
    {
        protected IEnumerator<TEntity> Enumerator { get; set; }

        protected Expression Expression { get; }

        protected IQueryExecutor Executor { get; }

        protected IQueryContext Context { get; }

        public EntityEnumerable(Expression query, IQueryExecutor executor, IQueryContext context)
        {
            Expression = query;
            Executor = executor;
            Context = context;
        }

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            return Enumerator ?? (Enumerator = Executor.Execute<TEntity>(Expression, Context));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TEntity>)this).GetEnumerator();
        }
    }
}
