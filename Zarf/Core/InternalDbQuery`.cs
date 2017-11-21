using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Zarf.Query;

namespace Zarf.Core
{
    public class InternalDbQuery<TEntity> : IInternalDbQuery<TEntity>
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

        public InternalDbQuery(IQueryProvider provider)
        {
            Provider = provider;
            Expression = Expression.Constant(this);
        }

        public InternalDbQuery(IQueryProvider provider, Expression query)
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
