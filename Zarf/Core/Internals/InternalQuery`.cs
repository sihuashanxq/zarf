using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf.Core.Internals
{
    internal class InternalQuery<TEntity> : IInternalQuery<TEntity>
    {
        private EntityEnumerable<TEntity> _entities;

        public DbContext Context => (Provider as QueryProvider)?.Context;

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public EntityEnumerable<TEntity> EntityEnumerable
        {
            get
            {
                if (_entities == null)
                {
                    _entities = new EntityEnumerable<TEntity>(Expression, Context.DbContextParts);
                }

                return _entities;
            }
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
            return EntityEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<object>)this).GetEnumerator();
        }
    }
}
