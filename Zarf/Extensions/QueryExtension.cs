using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Entities;

namespace Zarf
{
    public static class QueryExtension
    {
        public static void AddRange<TEntity>(this IQuery<TEntity> dbQuery, IEnumerable<TEntity> entities)
        {
            dbQuery.DbContext?.AddRange(entities);
        }

        public static void Add<TEntity>(this IQuery<TEntity> dbQuery, TEntity entity)
        {
            dbQuery.DbContext?.Add(entity);
        }

        public static void Update<TEntity>(this IQuery<TEntity> dbQuery, TEntity entity)
        {
            dbQuery.DbContext?.Update(entity);
        }

        public static void Update<TEntity>(this IQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            dbQuery.DbContext?.Update(entity, predicate);
        }

        public static void Delete<TEntity>(this IQuery<TEntity> dbQuery, Expression<Func<TEntity, bool>> predicate)
        {
            dbQuery.DbContext?.Delete(predicate);
        }

        public static void Delete<TEntity>(this IQuery<TEntity> dbQuery, TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            dbQuery.DbContext?.Delete(entity, predicate);
        }

        public static void Delete<TEntity>(this IQuery<TEntity> dbQuery, TEntity entity)
        {
            dbQuery.DbContext?.Delete(entity);
        }

        public static IJoinQuery<T1, T2> InnerJoin<T1, T2>(this IQuery<T1> dbQuery, IQuery<T2> joinQuery, Expression<Func<T1, T2, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Inner);
        }

        public static IJoinQuery<T1, T2> LeftJoin<T1, T2>(this IQuery<T1> dbQuery, IQuery<T2> joinQuery, Expression<Func<T1, T2, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Left);
        }

        public static IJoinQuery<T1, T2> RightJoin<T1, T2>(this IQuery<T1> dbQuery, IQuery<T2> joinQuery, Expression<Func<T1, T2, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Right);
        }

        public static IJoinQuery<T1, T2> FullJoin<T1, T2>(this IQuery<T1> dbQuery, IQuery<T2> joinQuery, Expression<Func<T1, T2, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Full);
        }

        public static IJoinQuery<T1, T2> CrossJoin<T1, T2>(this IQuery<T1> dbQuery, IQuery<T2> joinQuery, Expression<Func<T1, T2, bool>> predicate = null)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Cross);
        }

        public static IJoinQuery<T1, T2,T3> InnerJoin<T1, T2,T3>(this IJoinQuery<T1, T2> dbQuery, IQuery<T3> joinQuery, Expression<Func<T1, T2,T3, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Inner);
        }

        public static IJoinQuery<T1, T2, T3> LeftJoin<T1, T2, T3>(this IJoinQuery<T1, T2> dbQuery, IQuery<T3> joinQuery, Expression<Func<T1, T2, T3, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Left);
        }

        public static IJoinQuery<T1, T2, T3> RightJoin<T1, T2, T3>(this IJoinQuery<T1, T2> dbQuery, IQuery<T3> joinQuery, Expression<Func<T1, T2, T3, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Right);
        }

        public static IJoinQuery<T1, T2, T3> FullJoin<T1, T2, T3>(this IJoinQuery<T1, T2> dbQuery, IQuery<T3> joinQuery, Expression<Func<T1, T2, T3, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Full);
        }

        public static IJoinQuery<T1, T2, T3> CrossJoin<T1, T2, T3>(this IJoinQuery<T1, T2> dbQuery, IQuery<T3> joinQuery, Expression<Func<T1, T2,T3, bool>> predicate = null)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Cross);
        }

        public static IJoinQuery<T1, T2, T3,T4> InnerJoin<T1, T2, T3, T4>(this IJoinQuery<T1, T2, T3> dbQuery, IQuery<T4> joinQuery, Expression<Func<T1, T2, T3, T4, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Inner);
        }

        public static IJoinQuery<T1, T2, T3, T4> LeftJoin<T1, T2, T3, T4>(this IJoinQuery<T1, T2, T3> dbQuery, IQuery<T4> joinQuery, Expression<Func<T1, T2, T3, T4, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Left);
        }

        public static IJoinQuery<T1, T2, T3, T4> RightJoin<T1, T2, T3,T4>(this IJoinQuery<T1, T2, T3> dbQuery, IQuery<T4> joinQuery, Expression<Func<T1, T2, T3, T4, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Right);
        }

        public static IJoinQuery<T1, T2, T3, T4> FullJoin<T1, T2, T3, T4>(this IJoinQuery<T1, T2, T3> dbQuery, IQuery<T4> joinQuery, Expression<Func<T1, T2, T3, T4, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Full);
        }

        public static IJoinQuery<T1, T2, T3, T4> CrossJoin<T1, T2, T3, T4>(this IJoinQuery<T1, T2, T3> dbQuery, IQuery<T4> joinQuery, Expression<Func<T1, T2, T3, T4, bool>> predicate = null)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Cross);
        }

        public static IJoinQuery<T1, T2, T3, T4,T5> InnerJoin<T1, T2, T3, T4, T5>(this IJoinQuery<T1, T2, T3, T4> dbQuery, IQuery<T5> joinQuery, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Inner);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5> LeftJoin<T1, T2, T3, T4, T5>(this IJoinQuery<T1, T2, T3, T4> dbQuery, IQuery<T5> joinQuery, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Left);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5> RightJoin<T1, T2, T3, T4, T5>(this IJoinQuery<T1, T2, T3, T4> dbQuery, IQuery<T5> joinQuery, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Right);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5> FullJoin<T1, T2, T3, T4, T5>(this IJoinQuery<T1, T2, T3, T4> dbQuery, IQuery<T5> joinQuery, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Full);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5> CrossJoin<T1, T2, T3, T4, T5>(this IJoinQuery<T1, T2, T3, T4> dbQuery, IQuery<T5> joinQuery, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate = null)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Cross);
        }


        public static IJoinQuery<T1, T2, T3, T4, T5,T6> InnerJoin<T1, T2, T3, T4, T5, T6>(this IJoinQuery<T1, T2, T3, T4, T5> dbQuery, IQuery<T6> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Inner);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6> LeftJoin<T1, T2, T3, T4, T5, T6>(this IJoinQuery<T1, T2, T3, T4, T5> dbQuery, IQuery<T6> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Left);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6> RightJoin<T1, T2, T3, T4, T5, T6>(this IJoinQuery<T1, T2, T3, T4, T5> dbQuery, IQuery<T6> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Right);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6> FullJoin<T1, T2, T3, T4, T5, T6>(this IJoinQuery<T1, T2, T3, T4, T5> dbQuery, IQuery<T6> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Full);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6> CrossJoin<T1, T2, T3, T4, T5, T6>(this IJoinQuery<T1, T2, T3, T4, T5> dbQuery, IQuery<T6> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate = null)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Cross);
        }


        public static IJoinQuery<T1, T2, T3, T4, T5, T6,T7> InnerJoin<T1, T2, T3, T4, T5, T6, T7>(this IJoinQuery<T1, T2, T3, T4, T5, T6> dbQuery, IQuery<T7> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Inner);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin<T1, T2, T3, T4, T5, T6, T7>(this IJoinQuery<T1, T2, T3, T4, T5, T6> dbQuery, IQuery<T7> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Left);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin<T1, T2, T3, T4, T5, T6, T7>(this IJoinQuery<T1, T2, T3, T4, T5, T6> dbQuery, IQuery<T7> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Right);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7> FullJoin<T1, T2, T3, T4, T5, T6, T7>(this IJoinQuery<T1, T2, T3, T4, T5, T6> dbQuery, IQuery<T7> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Full);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7> CrossJoin<T1, T2, T3, T4, T5, T6, T7>(this IJoinQuery<T1, T2, T3, T4, T5, T6> dbQuery, IQuery<T7> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate = null)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Cross);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7,T8> InnerJoin<T1, T2, T3, T4, T5, T6, T7, T8>(this IJoinQuery<T1, T2, T3, T4, T5, T6, T7> dbQuery, IQuery<T8> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Inner);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T1, T2, T3, T4, T5, T6, T7, T8>(this IJoinQuery<T1, T2, T3, T4, T5, T6, T7> dbQuery, IQuery<T8> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Left);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin<T1, T2, T3, T4, T5, T6, T7, T8>(this IJoinQuery<T1, T2, T3, T4, T5, T6, T7> dbQuery, IQuery<T8> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Right);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> FullJoin<T1, T2, T3, T4, T5, T6, T7, T8>(this IJoinQuery<T1, T2, T3, T4, T5, T6, T7> dbQuery, IQuery<T8> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Full);
        }

        public static IJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> CrossJoin<T1, T2, T3, T4, T5, T6, T7, T8>(this IJoinQuery<T1, T2, T3, T4, T5, T6, T7> dbQuery, IQuery<T8> joinQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate = null)
        {
            return dbQuery.Join(joinQuery, predicate, JoinType.Cross);
        }
    }
}
