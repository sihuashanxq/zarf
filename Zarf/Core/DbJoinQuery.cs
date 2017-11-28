using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Entities;

namespace Zarf.Core
{
    public class JoinQueryEntry
    {
        public JoinType JoinType { get; }

        public Expression Predicate { get; }
    }

    internal class DbJoinQuery<T1, T2> : InternalDbQuery<T1>, IDbJoinQuery<T1, T2>
    {
        InternalDbQuery<T2> Inner { get; }
        //OR
        List<IInternalDbQuery> Joins { get; }

        public DbJoinQuery(IQueryProvider provider) : base(provider)
        {

        }

        public DbJoinQuery(IQueryProvider provider, Expression query) : base(provider, query)
        {
        }

        public IDbJoinQuery<T1, T2, T3> Join<T3>(IDbQuery<T3> query, Expression<Func<T1, T2, T3, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, TResult>> selector)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbJoinQuery<T1, T2, T3> : DbJoinQuery<T1, T2>, IDbJoinQuery<T1, T2, T3>
    {
        public DbJoinQuery(IQueryProvider provider) : base(provider)
        {

        }

        public IDbJoinQuery<T1, T2, T3, T4> Join<T4>(IDbQuery<T4> query, Expression<Func<T1, T2, T3, T4, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, TResult>> selector)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbJoinQuery<T1, T2, T3, T4> : DbJoinQuery<T1, T2, T3>, IDbJoinQuery<T1, T2, T3, T4>
    {
        public DbJoinQuery(IQueryProvider provider) : base(provider)
        {

        }

        public IDbJoinQuery<T1, T2, T3, T4, T5> Join<T5>(IDbQuery<T5> query, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> selector)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbJoinQuery<T1, T2, T3, T4, T5> : DbJoinQuery<T1, T2, T3, T4>, IDbJoinQuery<T1, T2, T3, T4, T5>
    {
        public DbJoinQuery(IQueryProvider provider) : base(provider)
        {

        }

        public IDbJoinQuery<T1, T2, T3, T4, T5, T6> Join<T6>(IDbQuery<T6> query, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> selector)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbJoinQuery<T1, T2, T3, T4, T5, T6> : DbJoinQuery<T1, T2, T3, T4, T5>, IDbJoinQuery<T1, T2, T3, T4, T5, T6>
    {
        public DbJoinQuery(IQueryProvider provider) : base(provider)
        {

        }

        public IDbJoinQuery<T1, T2, T3, T4, T5, T6, T7> Join<T7>(IDbQuery<T7> query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> selector)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbJoinQuery<T1, T2, T3, T4, T5, T6, T7> : DbJoinQuery<T1, T2, T3, T4, T5, T6>, IDbJoinQuery<T1, T2, T3, T4, T5, T6, T7>
    {
        public DbJoinQuery(IQueryProvider provider) : base(provider)
        {

        }

        public IDbJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> Join<T8>(IDbQuery<T8> query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> selector)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> : DbJoinQuery<T1, T2, T3, T4, T5, T6, T7>, IDbJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public DbJoinQuery(IQueryProvider provider) : base(provider)
        {

        }

        public IDbJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Join<T9>(IDbQuery<T9> query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> selector)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : DbJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8>, IDbJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        public DbJoinQuery(IQueryProvider provider) : base(provider)
        {

        }

        public IDbQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> selector)
        {
            throw new NotImplementedException();
        }
    }
}
