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
        /// <summary>
        /// 实体类型
        /// </summary>
        public Type ElementType => typeof(TEntity);

        /// <summary>
        /// 查询表达式
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// 查询提供者
        /// </summary>
        public IQueryProvider Provider { get; }

        public DataQuery(IQueryProvider provider)
        {
            Provider = provider;
            Expression = Expression.Constant(this);
        }

        public DataQuery(IQueryProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            var context = QueryContextFacotry.Factory.CreateContext() as QueryContext;
            var queryBuilder = new LinqExpressionTanslator(context);

            return new EntityEnumerable<TEntity>(queryBuilder.Build(Expression, context), context)
                .GetEnumerator();
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            var context = QueryContextFacotry.Factory.CreateContext() as QueryContext;
            var queryBuilder = new LinqExpressionTanslator(context);

            return new EntityEnumerable<object>(queryBuilder.Build(Expression, context), context)
                .GetEnumerator();
        }
    }
}
