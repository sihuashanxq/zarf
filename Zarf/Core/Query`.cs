using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Extensions;
using Zarf.Metadata.Entities;
using Zarf.Query;

namespace Zarf
{
    public class Query<TEntity> : Core.Query, IQuery<TEntity>
    {
        protected IInternalQuery<TEntity> EntityInternalQuery { get; set; }

        internal override IInternalQuery InternalQuery => EntityInternalQuery;

        public DbContext DbContext { get; }

        public Query(DbContext dbContext, IQueryExecutor executor)
        {
            DbContext = dbContext;
            EntityInternalQuery = new InternalQuery<TEntity>(new QueryProvider(DbContext, executor));
        }

        internal Query(IInternalQuery<TEntity> internalDbQuery)
        {
            EntityInternalQuery = internalDbQuery;
            DbContext = internalDbQuery.Provider.As<QueryProvider>().Context;
        }

        /// <summary>
        /// 实现GetEnumerator方法使Query<>支持foreach
        /// 不实现IEnumerable接口
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return EntityInternalQuery.GetEnumerator();
        }

        public IQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return new Query<TEntity>(EntityInternalQuery.Where(predicate) as IInternalQuery<TEntity>);
        }

        public List<TEntity> ToList()
        {
            return EntityInternalQuery.ToList();
        }

        public IEnumerable<TEntity> AsEnumerable()
        {
            return EntityInternalQuery.AsEnumerable();
        }

        public TEntity First()
        {
            return EntityInternalQuery.First();
        }

        public TEntity First(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityInternalQuery.First(predicate);
        }

        public TEntity FirstOrDefault()
        {
            return EntityInternalQuery.FirstOrDefault();
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityInternalQuery.FirstOrDefault(predicate);
        }

        public TEntity Single()
        {
            return EntityInternalQuery.Single();
        }

        public TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityInternalQuery.Single(predicate);
        }

        public TEntity SingleOrDefault()
        {
            return EntityInternalQuery.SingleOrDefault();
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityInternalQuery.SingleOrDefault(predicate);
        }

        public TEntity Last(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityInternalQuery.Last(predicate);
        }

        public TEntity Last()
        {
            return EntityInternalQuery.Last();
        }

        public TEntity LastOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityInternalQuery.LastOrDefault(predicate);
        }

        public TEntity LastOrDefault()
        {
            return EntityInternalQuery.LastOrDefault();
        }

        public IQuery<TEntity> Skip(int count)
        {
            return new Query<TEntity>(EntityInternalQuery.Skip(count) as IInternalQuery<TEntity>);
        }

        public IQuery<TEntity> Take(int count)
        {
            return new Query<TEntity>(EntityInternalQuery.Take(count) as IInternalQuery<TEntity>);
        }

        public bool All(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityInternalQuery.All(predicate);
        }

        public bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityInternalQuery.Any(predicate);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return new Query<TResult>(EntityInternalQuery.Select(selector) as IInternalQuery<TResult>);
        }

        public IQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            return new Query<TEntity>(EntityInternalQuery.OrderBy(keySelector) as IInternalQuery<TEntity>);
        }

        public IQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            return new Query<TEntity>(EntityInternalQuery.OrderByDescending(keySelector) as IInternalQuery<TEntity>);
        }

        public IQuery<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            return new Query<TEntity>(EntityInternalQuery.ThenBy(keySelector) as IInternalQuery<TEntity>);
        }

        public IQuery<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            return new Query<TEntity>(EntityInternalQuery.ThenByDescending(keySelector) as IInternalQuery<TEntity>);
        }

        public IQuery<TEntity> Distinct()
        {
            return new Query<TEntity>(EntityInternalQuery.Distinct() as IInternalQuery<TEntity>);
        }

        public IQuery<TEntity> Except(IQuery<TEntity> other)
        {
            return new Query<TEntity>(
                EntityInternalQuery.Except(
                    other.As<Core.Query>().InternalQuery.As<IInternalQuery<TEntity>>()) as IInternalQuery<TEntity>);
        }

        public IQuery<TEntity> Intersect(IQuery<TEntity> other)
        {
            return new Query<TEntity>(
                EntityInternalQuery.Intersect(
                    other.As<Core.Query>().InternalQuery.As<IInternalQuery<TEntity>>()) as IInternalQuery<TEntity>);
        }

        public IQuery<TEntity> Union(IQuery<TEntity> other)
        {
            return new Query<TEntity>(
                EntityInternalQuery.Union(
                    other.As<Core.Query>().InternalQuery.As<IInternalQuery<TEntity>>()) as IInternalQuery<TEntity>);
        }

        public int Sum(Expression<Func<TEntity, int>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public long Sum(Expression<Func<TEntity, long>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public decimal? Sum(Expression<Func<TEntity, decimal?>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public double? Sum(Expression<Func<TEntity, double?>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public float Sum(Expression<Func<TEntity, float>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public long? Sum(Expression<Func<TEntity, long?>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public float? Sum(Expression<Func<TEntity, float?>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public double Sum(Expression<Func<TEntity, double>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public int? Sum(Expression<Func<TEntity, int?>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public decimal Sum(Expression<Func<TEntity, decimal>> selector)
        {
            return EntityInternalQuery.Sum(selector);
        }

        public double Average(Expression<Func<TEntity, int>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public double Average(Expression<Func<TEntity, long>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public decimal? Average(Expression<Func<TEntity, decimal?>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, double?>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public float Average(Expression<Func<TEntity, float>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, long?>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public float? Average(Expression<Func<TEntity, float?>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public double Average(Expression<Func<TEntity, double>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, int?>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public decimal Average(Expression<Func<TEntity, decimal>> selector)
        {
            return EntityInternalQuery.Average(selector);
        }

        public IQuery<TEntity> Concat(IQuery<TEntity> other)
        {
            return new Query<TEntity>(
                EntityInternalQuery.Concat(
                    other.As<Core.Query>().InternalQuery.As<IInternalQuery<TEntity>>()) as IInternalQuery<TEntity>);
        }

        public int Count()
        {
            return EntityInternalQuery.Count();
        }

        public long LongCount()
        {
            return EntityInternalQuery.LongCount();
        }

        public TResult Max<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return EntityInternalQuery.Max(selector);
        }

        public TResult Min<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return EntityInternalQuery.Max(selector);
        }

        public IQuery<TResult> Join<TInner, TResult>(IQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate, JoinType joinType, Expression<Func<TEntity, TInner, TResult>> resultSelector)
        {
            return Join(inner, predicate, joinType).Select(resultSelector);
        }

        public IJoinQuery<TEntity, TInner> Join<TInner>(IQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            return new JoinQuery<TEntity, TInner>(JoinQuery.CreateJoinQuery(predicate, inner.As<Core.Query>().InternalQuery, joinType), InternalQuery);
        }

        public IQuery<TEntity> GroupBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            var groupExpression = Expression.Call(
                null,
                QueryableDefinition.Methods.FirstOrDefault(item => item.Name == "GroupBy").MakeGenericMethod(new[] { typeof(TEntity), typeof(TKey) }),
                EntityInternalQuery.Expression,
                keySelector);

            return new Query<TEntity>(EntityInternalQuery
                .Provider
                .CreateQuery<TEntity>(groupExpression)
                .As<IInternalQuery<TEntity>>()
            );
        }
    }
}
