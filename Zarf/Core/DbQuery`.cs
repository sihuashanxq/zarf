using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Entities;
using Zarf.Extensions;

namespace Zarf
{
    public class DbQuery<TEntity> : IDbQuery<TEntity>
    {
        public IInternalDbQuery<TEntity> InternalDbQuery { get; set; }

        public DbQuery(IQueryProvider provider)
        {
            InternalDbQuery = new InternalDbQuery<TEntity>(provider);
        }

        protected DbQuery(IInternalDbQuery<TEntity> internalDbQuery)
        {
            InternalDbQuery = internalDbQuery;
        }

        /// <summary>
        /// 通过实现GetEnumerator方法使DbQuery支持foreach访问
        /// 不实现IEnumerable接口,Linq API太多了
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return InternalDbQuery.GetEnumerator();
        }

        public IDbQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            InternalDbQuery = InternalDbQuery.Where(predicate) as IInternalDbQuery<TEntity>;
            return this;
        }

        public List<TEntity> ToList()
        {
            return InternalDbQuery.ToList();
        }

        public TEntity[] ToArray()
        {
            return InternalDbQuery.ToArray();
        }

        public IEnumerable<TEntity> AsEnumerable()
        {
            return InternalDbQuery.AsEnumerable();
        }

        public TEntity First()
        {
            return InternalDbQuery.First();
        }

        public TEntity First(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.First(predicate);
        }

        public TEntity FirstOrDefault()
        {
            return InternalDbQuery.FirstOrDefault();
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.FirstOrDefault(predicate);
        }

        public TEntity Single()
        {
            return InternalDbQuery.Single();
        }

        public TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.Single(predicate);
        }

        public TEntity SingleOrDefault()
        {
            return InternalDbQuery.SingleOrDefault();
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.SingleOrDefault(predicate);
        }

        public TEntity Last(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.Last(predicate);
        }

        public TEntity Last()
        {
            return InternalDbQuery.Last();
        }

        public TEntity LastOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.LastOrDefault(predicate);
        }

        public TEntity LastOrDefault()
        {
            return InternalDbQuery.LastOrDefault();
        }

        public IDbQuery<TEntity> Skip(int count)
        {
            InternalDbQuery = InternalDbQuery.Skip(count) as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> Take(int count)
        {
            InternalDbQuery = InternalDbQuery.Take(count) as IInternalDbQuery<TEntity>;
            return this;
        }

        public bool All(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.All(predicate);
        }

        public bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.Any(predicate);
        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return new DbQuery<TResult>(InternalDbQuery.Select(selector) as IInternalDbQuery<TResult>);
        }

        public IDbQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            InternalDbQuery = InternalDbQuery.OrderBy(keySelector) as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            InternalDbQuery = InternalDbQuery.OrderByDescending(keySelector) as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            InternalDbQuery = InternalDbQuery.ThenBy(keySelector) as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            InternalDbQuery = InternalDbQuery.ThenByDescending(keySelector) as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> GroupBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            InternalDbQuery = InternalDbQuery.GroupBy(keySelector) as IInternalDbQuery<TEntity>;
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
        public IIncludeDbQuery<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> propertyPath, Expression<Func<TEntity, TProperty, bool>> propertyRelation)
        {
            return new IncludeDbQuery<TEntity, TProperty>(
                InternalDbQuery.Include(
                    propertyPath,
                    propertyRelation ?? CreateDeafultKeyRealtion<TEntity, TProperty>()
                    )
                );
        }

        public IIncludeDbQuery<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> propertyPath)
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

        public IDbQuery<TEntity> DefaultIfEmpty()
        {
            InternalDbQuery = InternalDbQuery.DefaultIfEmpty() as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> Distinct()
        {
            InternalDbQuery = InternalDbQuery.Distinct() as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> Except(IDbQuery<TEntity> other)
        {
            InternalDbQuery = InternalDbQuery.Except(other.InternalDbQuery) as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> Intersect(IDbQuery<TEntity> other)
        {
            InternalDbQuery = InternalDbQuery.Intersect(other.InternalDbQuery) as IInternalDbQuery<TEntity>;
            return this;
        }

        public IDbQuery<TEntity> Union(IDbQuery<TEntity> other)
        {
            InternalDbQuery = InternalDbQuery.Union(other.InternalDbQuery) as IInternalDbQuery<TEntity>;
            return this;
        }

        public int Sum(Expression<Func<TEntity, int>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public long Sum(Expression<Func<TEntity, long>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public decimal? Sum(Expression<Func<TEntity, decimal?>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public double? Sum(Expression<Func<TEntity, double?>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public float Sum(Expression<Func<TEntity, float>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public long? Sum(Expression<Func<TEntity, long?>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public float? Sum(Expression<Func<TEntity, float?>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public double Sum(Expression<Func<TEntity, double>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public int? Sum(Expression<Func<TEntity, int?>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public decimal Sum(Expression<Func<TEntity, decimal>> selector)
        {
            return InternalDbQuery.Sum(selector);
        }

        public double Average(Expression<Func<TEntity, int>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public double Average(Expression<Func<TEntity, long>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public decimal? Average(Expression<Func<TEntity, decimal?>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, double?>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public float Average(Expression<Func<TEntity, float>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, long?>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public float? Average(Expression<Func<TEntity, float?>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public double Average(Expression<Func<TEntity, double>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public double? Average(Expression<Func<TEntity, int?>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public decimal Average(Expression<Func<TEntity, decimal>> selector)
        {
            return InternalDbQuery.Average(selector);
        }

        public IDbQuery<TEntity> Concat(IDbQuery<TEntity> other)
        {
            InternalDbQuery = InternalDbQuery.Concat(other.InternalDbQuery) as IInternalDbQuery<TEntity>;
            return this;
        }

        public int Count()
        {
            return InternalDbQuery.Count();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.Count(predicate);
        }

        public long LongCount()
        {
            return InternalDbQuery.LongCount();
        }

        public long LongCount(Expression<Func<TEntity, bool>> predicate)
        {
            return InternalDbQuery.LongCount(predicate);
        }

        public TResult Max<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return InternalDbQuery.Max(selector);
        }

        public TResult Min<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return InternalDbQuery.Max(selector);
        }

        public IDbQuery<TResult> Join<TInner, TKey, TResult>(IDbQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate, Expression<Func<TEntity, TInner, TResult>> resultSelector)
        {
            throw new NotImplementedException();
        }

        public IDbJoinQuery<TEntity, TInner> Join<TInner>(IDbQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IDbQuery<TResult> Join<TInner, TKey, TResult>(IDbQuery<TInner> inner, Expression<Func<TEntity, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TEntity, TInner, TResult>> resultSelector)
        {
            return new DbQuery<TResult>(InternalDbQuery.Join(inner.InternalDbQuery, outerKeySelector, innerKeySelector, resultSelector) as IInternalDbQuery<TResult>);
        }
    }
}
