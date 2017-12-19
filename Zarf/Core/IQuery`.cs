using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Zarf.Core;
using Zarf.Entities;
using Zarf.Core.Internals;

namespace Zarf
{
    public interface IQuery
    {
        IInternalQuery GetInternalQuery();
    }

    public interface IQuery<TEntity> : IQuery
    {
        DbContext DbContext { get; }

        IInternalQuery<TEntity> InternalQuery { get; }

        IQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        TEntity First();

        TEntity First(Expression<Func<TEntity, bool>> predicate);

        TEntity FirstOrDefault();

        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

        TEntity Single();

        TEntity Single(Expression<Func<TEntity, bool>> predicate);

        TEntity SingleOrDefault();

        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);

        TEntity Last(Expression<Func<TEntity, bool>> predicate);

        TEntity Last();

        TEntity LastOrDefault(Expression<Func<TEntity, bool>> predicate);

        TEntity LastOrDefault();

        IQuery<TEntity> Skip(int count);

        IQuery<TEntity> Take(int count);

        IQuery<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector);

        bool All(Expression<Func<TEntity, bool>> predicate);

        bool Any(Expression<Func<TEntity, bool>> predicate);

        IQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IQuery<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IQuery<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IQuery<TEntity> GroupBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IQuery<TResult> Join<TInner, TResult>(IQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate, JoinType joinType, Expression<Func<TEntity, TInner, TResult>> resultSelector);

        IJoinQuery<TEntity, TInner> Join<TInner>(IQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate, JoinType joinType = JoinType.Inner);

        IQuery<TEntity> Distinct();

        IQuery<TEntity> Except(IQuery<TEntity> source2);

        IQuery<TEntity> Intersect(IQuery<TEntity> source2);

        IQuery<TEntity> Union(IQuery<TEntity> source2);

        IIncludeQuery<TEntity, TKey> Include<TKey>(Expression<Func<TEntity, IEnumerable<TKey>>> propertyPath, Expression<Func<TEntity, TKey, bool>> propertyRelation);

        IIncludeQuery<TEntity, TKey> Include<TKey>(Expression<Func<TEntity, IEnumerable<TKey>>> propertyPath);

        int Sum(Expression<Func<TEntity, int>> selector);

        long Sum(Expression<Func<TEntity, long>> selector);

        decimal? Sum(Expression<Func<TEntity, decimal?>> selector);

        double? Sum(Expression<Func<TEntity, double?>> selector);

        float Sum(Expression<Func<TEntity, float>> selector);

        long? Sum(Expression<Func<TEntity, long?>> selector);

        float? Sum(Expression<Func<TEntity, float?>> selector);

        double Sum(Expression<Func<TEntity, double>> selector);

        int? Sum(Expression<Func<TEntity, int?>> selector);

        decimal Sum(Expression<Func<TEntity, decimal>> selector);

        double Average(Expression<Func<TEntity, int>> selector);

        double Average(Expression<Func<TEntity, long>> selector);

        decimal? Average(Expression<Func<TEntity, decimal?>> selector);

        double? Average(Expression<Func<TEntity, double?>> selector);

        float Average(Expression<Func<TEntity, float>> selector);

        double? Average(Expression<Func<TEntity, long?>> selector);

        float? Average(Expression<Func<TEntity, float?>> selector);

        double Average(Expression<Func<TEntity, double>> selector);

        double? Average(Expression<Func<TEntity, int?>> selector);

        decimal Average(Expression<Func<TEntity, decimal>> selector);

        IQuery<TEntity> Concat(IQuery<TEntity> source2);

        int Count();

        long LongCount();

        TResult Max<TResult>(Expression<Func<TEntity, TResult>> selector);

        TResult Min<TResult>(Expression<Func<TEntity, TResult>> selector);

        IEnumerable<TEntity> AsEnumerable();

        List<TEntity> ToList();

        TEntity[] ToArray();

        IEnumerator<TEntity> GetEnumerator();
    }
}
