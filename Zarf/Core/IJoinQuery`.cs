using System;
using System.Linq.Expressions;
using Zarf.Entities;

namespace Zarf.Core
{
    public interface IJoinQuery
    {

    }

    public interface IJoinQuery<T1, T2> : IJoinQuery
    {
        IJoinQuery<T1, T2, T3> Join<T3>(IDbQuery<T3> query, Expression<Func<T1, T2, T3, bool>> predicate, JoinType joinType = JoinType.Inner);

        IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, TResult>> selector);
    }

    public interface IJoinQuery<T1, T2, T3> : IJoinQuery
    {
        IJoinQuery<T1, T2, T3, T4> Join<T4>(IDbQuery<T4> query, Expression<Func<T1, T2, T3, T4, bool>> predicate, JoinType joinType = JoinType.Inner);

        IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, TResult>> selector);
    }

    public interface IJoinQuery<T1, T2, T3, T4> : IJoinQuery
    {
        IJoinQuery<T1, T2, T3, T4, T5> Join<T5>(IDbQuery<T5> query, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, JoinType joinType = JoinType.Inner);

        IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> selector);
    }

    public interface IJoinQuery<T1, T2, T3, T4, T5> : IJoinQuery
    {
        IJoinQuery<T1, T2, T3, T4, T5, T6> Join<T6>(IDbQuery<T6> query, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, JoinType joinType = JoinType.Inner);

        IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> selector);
    }

    public interface IJoinQuery<T1, T2, T3, T4, T5, T6> : IJoinQuery
    {
        IJoinQuery<T1, T2, T3, T4, T5, T6, T7> Join<T7>(IDbQuery<T7> query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate, JoinType joinType = JoinType.Inner);

        IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> selector);
    }

    public interface IJoinQuery<T1, T2, T3, T4, T5, T6, T7> : IJoinQuery
    {
        IJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> Join<T8>(IDbQuery<T8> query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate, JoinType joinType = JoinType.Inner);

        IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> selector);
    }

    public interface IJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> : IJoinQuery
    {
        IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> selector);
    }
}
