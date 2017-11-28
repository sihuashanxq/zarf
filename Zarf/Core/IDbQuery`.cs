using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Zarf.Core;

namespace Zarf
{
    public interface IDbQuery<TEntity>
    {
        IInternalDbQuery<TEntity> InternalDbQuery { get; set; }

        IDbQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

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

        IDbQuery<TEntity> Skip(int count);

        IDbQuery<TEntity> Take(int count);

        IDbQuery<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector);

        bool All(Expression<Func<TEntity, bool>> predicate);

        bool Any(Expression<Func<TEntity, bool>> predicate);

        IDbQuery<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IDbQuery<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IDbQuery<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IDbQuery<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IDbQuery<TEntity> GroupBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);

        IDbQuery<TResult> Join<TInner, TKey, TResult>(IDbQuery<TInner> inner, Expression<Func<TEntity, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TEntity, TInner, TResult>> resultSelector);

        IDbQuery<TResult> Join<TInner, TKey, TResult>(IDbQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate, Expression<Func<TEntity, TInner, TResult>> resultSelector);

        IDbJoinQuery<TEntity, TInner> Join<TInner>(IDbQuery<TInner> inner, Expression<Func<TEntity, TInner, bool>> predicate);

        IDbQuery<TEntity> DefaultIfEmpty();

        IDbQuery<TEntity> Distinct();

        IDbQuery<TEntity> Except(IDbQuery<TEntity> source2);

        IDbQuery<TEntity> Intersect(IDbQuery<TEntity> source2);

        IDbQuery<TEntity> Union(IDbQuery<TEntity> source2);

        IIncludeDbQuery<TEntity, TKey> Include<TKey>(Expression<Func<TEntity, IEnumerable<TKey>>> propertyPath, Expression<Func<TEntity, TKey, bool>> propertyRelation);

        IIncludeDbQuery<TEntity, TKey> Include<TKey>(Expression<Func<TEntity, IEnumerable<TKey>>> propertyPath);

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

        IDbQuery<TEntity> Concat(IDbQuery<TEntity> source2);

        int Count();

        int Count(Expression<Func<TEntity, bool>> predicate);

        long LongCount();

        long LongCount(Expression<Func<TEntity, bool>> predicate);

        TResult Max<TResult>(Expression<Func<TEntity, TResult>> selector);

        TResult Min<TResult>(Expression<Func<TEntity, TResult>> selector);

        IEnumerable<TEntity> AsEnumerable();

        List<TEntity> ToList();

        TEntity[] ToArray();

        IEnumerator<TEntity> GetEnumerator();
    }
}
