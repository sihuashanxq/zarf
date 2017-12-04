using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Entities;
using Zarf.Extensions;

namespace Zarf
{
    public class Query<TEntity> : IQuery<TEntity>
    {
        private IInternalQuery<TEntity> _internalQuery;

        public IInternalQuery<TEntity> InternalQuery => _internalQuery;

        public DbContext DbContext { get; }

        public Query(DbContext dbContext)
        {
            DbContext = dbContext;
            _internalQuery = new InternalQuery<TEntity>(new QueryProvider(DbContext));
        }

        internal Query(IInternalQuery<TEntity> internalDbQuery)
        {
            _internalQuery = internalDbQuery;
            DbContext = internalDbQuery.Provider.As<QueryProvider>().Context;
        }

        /// <summary>
        /// 通过实现GetEnumerator方法使DbQuery支持foreach访问
        /// 不实现IEnumerable接口,Linq API太多了
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return InternalQuery.GetEnumerator();
        }

        public IQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            _internalQuery = InternalQuery.Where(predicate) as IInternalQuery<TEntity>;
            return this;
        }

        public List<TEntity> ToList()
        {
            return InternalQuery.ToList();
        }

        public TEntity[] ToArray()
        {
            return InternalQuery.ToArray();
        }

        public IEnumerable<TEntity> AsEnumerable()
        {
            return InternalQuery.AsEnumerable();
        }

        public TEntity First()
        {
            return InternalQuery.First();
        }

        public TEntity First(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.First(predicate);
        }

        public TEntity FirstOrDefault()
        {
            return InternalQuery.FirstOrDefault();
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.FirstOrDefault(predicate);
        }

        public TEntity Single()
        {
            return InternalQuery.Single();
        }

        public TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.Single(predicate);
        }

        public TEntity SingleOrDefault()
        {
            return InternalQuery.SingleOrDefault();
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.SingleOrDefault(predicate);
        }

        public TEntity Last(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.Last(predicate);
        }

        public TEntity Last()
        {
            return InternalQuery.Last();
        }

        public TEntity LastOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.LastOrDefault(predicate);
        }

        public TEntity LastOrDefault()
        {
            return InternalQuery.LastOrDefault();
        }

        public IQuery<TEntity> Skip(int count)
        {
            _internalQuery = InternalQuery.Skip(count) as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> Take(int count)
        {
            _internalQuery = InternalQuery.Take(count) as IInternalQuery<TEntity>;
            return this;
        }

        public bool All(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.All(predicate);
        }

        public bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.Any(predicate);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return new Query<TResult>(InternalQuery.Select(selector) as IInternalQuery<TResult>);
        }

        public IQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            _internalQuery = InternalQuery.OrderBy(keySelector) as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            _internalQuery = InternalQuery.OrderByDescending(keySelector) as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            _internalQuery = InternalQuery.ThenBy(keySelector) as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            _internalQuery = InternalQuery.ThenByDescending(keySelector) as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> GroupBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            _internalQuery = InternalQuery.GroupBy(keySelector) as IInternalQuery<TEntity>;
            return this;
        }

        /// <summary>
        /// 查询包含属性
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="dbQuery">原始查询</param>
        /// <param name="propertyPath">属性路径</param>
        /// <param name="propertyRelation">关联关系</param>
        public IIncludeQuery<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> propertyPath, Expression<Func<TEntity, TProperty, bool>> propertyRelation)
        {
            return new IncludeQuery<TEntity, TProperty>(
                InternalQuery.Include(
                    propertyPath,
                    propertyRelation ?? CreateDeafultKeyRealtion<TEntity, TProperty>()
                    )
                );
        }

        public IIncludeQuery<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> propertyPath)
        {
            return Include(propertyPath, null);
        }

        protected Expression<Func<TTEntity, TProperty, bool>> CreateDeafultKeyRealtion<TTEntity, TProperty>()
        {
            var typeOfEntity = typeof(TTEntity);
            var typeOfProperty = typeof(TProperty);

            var idOfEntity = typeOfEntity.GetMembers().FirstOrDefault(item => item.GetCustomAttribute<PrimaryKeyAttribute>() != null || item.Name == "Id");
            var foreignKeyOfProperty = FindEntityForeignKey(typeOfProperty, typeOfEntity.Name + "Id");

            if (idOfEntity == null)
            {
                throw new NotImplementedException($"Type Of {typeOfEntity.FullName} Need A PrimaryKey Or Id Member");
            }

            if (foreignKeyOfProperty == null)
            {
                throw new NotImplementedException($"Type Of {typeOfProperty.FullName} Have Not A Foreign Key With{typeOfEntity.FullName}");
            }

            var oEntity = Expression.Parameter(typeOfEntity);
            var oProperty = Expression.Parameter(typeOfProperty);

            return
                Expression.Lambda<Func<TTEntity, TProperty, bool>>(
                    Expression.Equal(
                        Expression.MakeMemberAccess(oEntity, idOfEntity),
                        Expression.MakeMemberAccess(oProperty, foreignKeyOfProperty)
                    ),
                    new[] { oEntity, oProperty }
            );
        }

        protected MemberInfo FindEntityForeignKey(Type typeOfEntity, string defaultKey)
        {
            var foreignKey = typeOfEntity.GetMembers().FirstOrDefault(item => item.GetCustomAttribute<ForeignKeyAttribute>()?.Name == defaultKey);
            if (foreignKey == null)
            {
                return typeOfEntity.GetMembers().FirstOrDefault(item => item.ToColumn().Name == defaultKey);
            }

            return foreignKey;
        }

        public IQuery<TEntity> DefaultIfEmpty()
        {
            _internalQuery = InternalQuery.DefaultIfEmpty() as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> Distinct()
        {
            _internalQuery = InternalQuery.Distinct() as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> Except(IQuery<TEntity> other)
        {
            _internalQuery = InternalQuery.Except(other.InternalQuery) as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> Intersect(IQuery<TEntity> other)
        {
            _internalQuery = InternalQuery.Intersect(other.InternalQuery) as IInternalQuery<TEntity>;
            return this;
        }

        public IQuery<TEntity> Union(IQuery<TEntity> other)
        {
            _internalQuery = InternalQuery.Union(other.InternalQuery) as IInternalQuery<TEntity>;
            return this;
        }

        public int Sum(Expression<Func<TEntity, int>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public long Sum(Expression<Func<TEntity, long>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public decimal? Sum(Expression<Func<TEntity, decimal?>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public double? Sum(Expression<Func<TEntity, double?>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public float Sum(Expression<Func<TEntity, float>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public long? Sum(Expression<Func<TEntity, long?>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public float? Sum(Expression<Func<TEntity, float?>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public double Sum(Expression<Func<TEntity, double>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public int? Sum(Expression<Func<TEntity, int?>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public decimal Sum(Expression<Func<TEntity, decimal>> selector)
        {
            return InternalQuery.Sum(selector);
        }

        public double Average(Expression<Func<TEntity, int>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public double Average(Expression<Func<TEntity, long>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public decimal? Average(Expression<Func<TEntity, decimal?>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, double?>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public float Average(Expression<Func<TEntity, float>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, long?>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public float? Average(Expression<Func<TEntity, float?>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public double Average(Expression<Func<TEntity, double>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, int?>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public decimal Average(Expression<Func<TEntity, decimal>> selector)
        {
            return InternalQuery.Average(selector);
        }

        public IQuery<TEntity> Concat(IQuery<TEntity> other)
        {
            _internalQuery = InternalQuery.Concat(other.InternalQuery) as IInternalQuery<TEntity>;
            return this;
        }

        public int Count()
        {
            return InternalQuery.Count();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.Count(predicate);
        }

        public long LongCount()
        {
            return InternalQuery.LongCount();
        }

        public long LongCount(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalQuery.LongCount(predicate);
        }

        public TResult Max<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return InternalQuery.Max(selector);
        }

        public TResult Min<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return InternalQuery.Max(selector);
        }

        public IQuery<TResult> Join<TInner, TResult>(IQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate, JoinType joinType, Expression<Func<TEntity, TInner, TResult>> resultSelector)
        {
            return Join(inner, predicate, joinType).Select(resultSelector);
        }

        public IJoinQuery<TEntity, TInner> Join<TInner>(IQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            return new JoinQuery<TEntity, TInner>(JoinQuery.CreateJoinQuery(predicate, inner.InternalQuery, joinType), InternalQuery);
        }
    }
}
