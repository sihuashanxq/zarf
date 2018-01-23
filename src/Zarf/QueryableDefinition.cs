using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf
{
    public static class QueryableDefinition
    {
        public static MethodInfo[] Methods = typeof(QueryableDefinition).GetMethods(BindingFlags.Public | BindingFlags.Static);

        public static bool All<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static bool Any<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static bool Any<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static double Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, int>> selector)
        {
            throw new NotImplementedException();
        }

        public static double Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, long>> selector)
        {
            throw new NotImplementedException();
        }

        public static decimal? Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, decimal?>> selector)
        {
            throw new NotImplementedException();
        }

        public static double? Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, double?>> selector)
        {
            throw new NotImplementedException();
        }

        public static float Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, float>> selector)
        {
            throw new NotImplementedException();
        }

        public static double? Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, long?>> selector)
        {
            throw new NotImplementedException();
        }

        public static float? Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public static double Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, double>> selector)
        {
            throw new NotImplementedException();
        }

        public static double? Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, int?>> selector)
        {
            throw new NotImplementedException();
        }

        public static decimal Average<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, decimal>> selector)
        {
            throw new NotImplementedException();
        }

        public static double? Average(IQueryable<int?> query)
        {
            throw new NotImplementedException();
        }

        public static float? Average(IQueryable<float?> query)
        {
            throw new NotImplementedException();
        }

        public static double? Average(IQueryable<long?> query)
        {
            throw new NotImplementedException();
        }

        public static double? Average(IQueryable<double?> query)
        {
            throw new NotImplementedException();
        }

        public static decimal? Average(IQueryable<decimal?> query)
        {
            throw new NotImplementedException();
        }

        public static double Average(IQueryable<long> query)
        {
            throw new NotImplementedException();
        }

        public static double Average(IQueryable<int> query)
        {
            throw new NotImplementedException();
        }

        public static double Average(IQueryable<double> query)
        {
            throw new NotImplementedException();
        }

        public static decimal Average(IQueryable<decimal> query)
        {
            throw new NotImplementedException();
        }

        public static float Average(IQueryable<float> query)
        {
            throw new NotImplementedException();
        }

        public static int Count<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static int Count<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> Distinct<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> Except<TEntity>(IQueryable<TEntity> source1, IEnumerable<TEntity> source2)
        {
            throw new NotImplementedException();
        }

        public static TEntity First<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static TEntity First<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static TEntity FirstOrDefault<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static TEntity FirstOrDefault<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> GroupBy<TEntity, TKey>(IQueryable<TEntity> query, Expression<Func<TEntity, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> Intersect<TEntity>(IQueryable<TEntity> source1, IEnumerable<TEntity> source2)
        {
            throw new NotImplementedException();
        }

        public static TEntity Last<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static TEntity Last<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static TEntity LastOrDefault<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        { throw new NotImplementedException(); }

        public static TEntity LastOrDefault<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static long LongCount<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static long LongCount<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        { throw new NotImplementedException(); }

        public static TResult Max<TEntity, TResult>(IQueryable<TEntity> query, Expression<Func<TEntity, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public static TResult Min<TEntity, TResult>(IQueryable<TEntity> query, Expression<Func<TEntity, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> OrderBy<TEntity, TKey>(IQueryable<TEntity> query, Expression<Func<TEntity, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> OrderByDescending<TEntity, TKey>(IQueryable<TEntity> query, Expression<Func<TEntity, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TResult> Select<TEntity, TResult>(IQueryable<TEntity> query, Expression<Func<TEntity, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public static TEntity Single<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static TEntity Single<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static TEntity SingleOrDefault<TEntity>(IQueryable<TEntity> query)
        {
            throw new NotImplementedException();
        }

        public static TEntity SingleOrDefault<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> Skip<TEntity>(IQueryable<TEntity> query, int count)
        {
            throw new NotImplementedException();
        }

        public static int Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, int>> selector)
        {
            throw new NotImplementedException();
        }

        public static long Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, long>> selector)
        {
            throw new NotImplementedException();
        }

        public static decimal? Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, decimal?>> selector)
        {
            throw new NotImplementedException();
        }

        public static double? Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, double?>> selector)
        {
            throw new NotImplementedException();
        }

        public static float Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, float>> selector)
        {
            throw new NotImplementedException();
        }

        public static long? Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, long?>> selector)
        {
            throw new NotImplementedException();
        }

        public static float? Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public static double Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, double>> selector)
        {
            throw new NotImplementedException();
        }

        public static int? Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, int?>> selector)
        {
            throw new NotImplementedException();
        }

        public static decimal Sum<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, decimal>> selector)
        {
            throw new NotImplementedException();
        }

        public static double Sum(IQueryable<double> query)
        {
            throw new NotImplementedException();
        }


        public static float? Sum(IQueryable<float?> query)
        {
            throw new NotImplementedException();
        }

        public static long? Sum(IQueryable<long?> query)
        {
            throw new NotImplementedException();
        }

        public static int? Sum(IQueryable<int?> query)
        {
            throw new NotImplementedException();
        }

        public static double? Sum(IQueryable<double?> query)
        {
            throw new NotImplementedException();
        }

        public static decimal? Sum(IQueryable<decimal?> query)
        {
            throw new NotImplementedException();
        }

        public static long Sum(IQueryable<long> query)
        {
            throw new NotImplementedException();
        }

        public static int Sum(IQueryable<int> query)
        {
            throw new NotImplementedException();
        }

        public static float Sum(IQueryable<float> query)
        {
            throw new NotImplementedException();
        }

        public static decimal Sum(IQueryable<decimal> query)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> Take<TEntity>(IQueryable<TEntity> query, int count)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> ThenBy<TEntity, TKey>(IOrderedQueryable<TEntity> query, Expression<Func<TEntity, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> ThenByDescending<TEntity, TKey>(IOrderedQueryable<TEntity> query, Expression<Func<TEntity, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> Union<TEntity>(IQueryable<TEntity> source1, IEnumerable<TEntity> source2)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TEntity> Where<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
