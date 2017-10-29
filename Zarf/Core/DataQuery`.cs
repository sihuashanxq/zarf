using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Query;

namespace Zarf
{
    /// <summary>
    /// 查询查询集合
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class DataQuery<TEntity> : IDataQuery<TEntity>
    {
        private EntityEnumerable<TEntity> _entityEnumerable;

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public EntityEnumerable<TEntity> EntityEnumerable
        {
            get
            {
                if (_entityEnumerable == null)
                {
                    _entityEnumerable = new EntityEnumerable<TEntity>(Expression);
                }

                return _entityEnumerable;
            }
        }

        public DataQuery(IQueryProvider provider)
        {
            Provider = provider;
            Expression = Expression.Constant(this);
        }

        public DataQuery(IQueryProvider provider, Expression exp)
        {
            Provider = provider;
            Expression = exp;
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
