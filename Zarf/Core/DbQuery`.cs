using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Zarf
{
    public class DbQuery<TEntity> : IDbQuery<TEntity>
    {
        private EntityEnumerable<TEntity> _entityEnumerable;

        public DbContext Context => (Provider as DbQueryProvider)?.Context;

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public EntityEnumerable<TEntity> EntityEnumerable
        {
            get
            {
                if (_entityEnumerable == null)
                {
                    _entityEnumerable = new EntityEnumerable<TEntity>(Expression, Context.DbContextParts);
                }

                return _entityEnumerable;
            }
        }

        public DbQuery(IQueryProvider provider)
        {
            Provider = provider;
            Expression = Expression.Constant(this);
        }

        public DbQuery(IQueryProvider provider, Expression query)
        {
            Provider = provider;
            Expression = query;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return EntityEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<object>)this).GetEnumerator();
        }
    }
}
