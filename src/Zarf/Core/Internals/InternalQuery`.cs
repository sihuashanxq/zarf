using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf.Core.Internals
{
    public class InternalQuery<TEntity> : IInternalQuery<TEntity>
    {
        public DbContext Context => (Provider as QueryProvider)?.Context;

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        protected EntityEnumerable<TEntity> InternalEnumerable
        {
            get
            {
                return new EntityEnumerable<TEntity>(
                        Expression,
                        Context.QueryExecutor,
                        Context.QueryContextFactory.CreateContext(Context));
            }
        }

        public Expression GetExpression()
        {
            return Expression;
        }

        public InternalQuery(IQueryProvider provider)
        {
            Provider = provider;
            Expression = Expression.Constant(this);
        }

        public InternalQuery(IQueryProvider provider, Expression query)
        {
            Provider = provider;
            Expression = query;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return InternalEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<object>)this).GetEnumerator();
        }
    }
}
