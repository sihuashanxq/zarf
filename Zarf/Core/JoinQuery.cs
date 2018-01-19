using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Core.Internals;
using Zarf.Entities;
using Zarf.Extensions;

namespace Zarf.Core
{
    public class JoinQuery : IJoinQuery
    {
        public List<JoinQuery> Joins { get; set; }

        public LambdaExpression Predicate { get; set; }

        public JoinType JoinType { get; set; }

        public IInternalQuery InternalDbQuery { get; set; }

        public static JoinQuery CreateJoinQuery(LambdaExpression predicate, IInternalQuery dbQuery, JoinType joinType)
        {
            return new JoinQuery()
            {
                Predicate = predicate,
                JoinType = joinType,
                InternalDbQuery = dbQuery
            };
        }

        internal static IQuery<TResult> JoinSelect<TResult>(JoinQuery join, LambdaExpression selector)
        {
            return new Query<TResult>(Select(Join<TResult>(join.InternalDbQuery as IQueryable, join.Joins), selector));
        }

        internal static IInternalQuery<TResult> Select<TResult>(IInternalQuery<TResult> dbQuery, LambdaExpression selector)
        {
            return new InternalQuery<TResult>(
               dbQuery.Provider,
               Expression.Call(
                   ReflectionUtil.JoinSelect.MakeGenericMethod(typeof(TResult)),
                   dbQuery.Expression,
                   selector
                   )
               );
        }

        internal static IInternalQuery<TResult> Join<TResult>(IQueryable dbQuery, List<JoinQuery> joins)
        {
            var query = dbQuery.GetType().GetProperty("Expression").GetValue(dbQuery) as Expression;
            var provider = dbQuery.GetType().GetProperty("Provider").GetValue(dbQuery) as IQueryProvider;

            return new InternalQuery<TResult>(
                provider,
                Expression.Call(
                    ReflectionUtil.Join.MakeGenericMethod(typeof(TResult)),
                    query,
                    Expression.Constant(joins)
                    )
                );
        }
    }

    internal class JoinQuery<T1, T2> : JoinQuery, IJoinQuery<T1, T2>
    {
        public JoinQuery()
        {
        }

        public JoinQuery(JoinQuery joinQuery, IInternalQuery dbQuery)
        {
            InternalDbQuery = dbQuery;
            Joins = new List<JoinQuery>() { joinQuery };
        }

        public IJoinQuery<T1, T2, T3> Join<T3>(IQuery<T3> query, Expression<Func<T1, T2, T3, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            Joins.Add(CreateJoinQuery(predicate, query.As<Query>().InternalQuery, joinType));
            return new JoinQuery<T1, T2, T3>(Joins, InternalDbQuery);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, TResult>> selector)
        {
            return JoinSelect<TResult>(this, selector);
        }
    }

    internal class JoinQuery<T1, T2, T3> : JoinQuery<T1, T2>, IJoinQuery<T1, T2, T3>
    {
        public JoinQuery(List<JoinQuery> joins, IInternalQuery dbQuery)
        {
            Joins = joins;
            InternalDbQuery = dbQuery;
        }

        public IJoinQuery<T1, T2, T3, T4> Join<T4>(IQuery<T4> query, Expression<Func<T1, T2, T3, T4, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            Joins.Add(CreateJoinQuery(predicate, query.As<Query>().InternalQuery, joinType));
            return new JoinQuery<T1, T2, T3, T4>(Joins, InternalDbQuery);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, TResult>> selector)
        {
            return JoinSelect<TResult>(this, selector);
        }
    }

    internal class JoinQuery<T1, T2, T3, T4> : JoinQuery<T1, T2, T3>, IJoinQuery<T1, T2, T3, T4>
    {
        public JoinQuery(List<JoinQuery> joins, IInternalQuery dbQuery)
            : base(joins, dbQuery)
        {
        }

        public IJoinQuery<T1, T2, T3, T4, T5> Join<T5>(IQuery<T5> query, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            Joins.Add(CreateJoinQuery(predicate, query.As<Query>().InternalQuery, joinType));
            return new JoinQuery<T1, T2, T3, T4, T5>(Joins, InternalDbQuery);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> selector)
        {
            return JoinSelect<TResult>(this, selector);
        }
    }

    internal class JoinQuery<T1, T2, T3, T4, T5> : JoinQuery<T1, T2, T3, T4>, IJoinQuery<T1, T2, T3, T4, T5>
    {
        public JoinQuery(List<JoinQuery> joins, IInternalQuery dbQuery)
            : base(joins, dbQuery)
        {
        }

        public IJoinQuery<T1, T2, T3, T4, T5, T6> Join<T6>(IQuery<T6> query, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            Joins.Add(CreateJoinQuery(predicate, query.As<Query>().InternalQuery, joinType));
            return new JoinQuery<T1, T2, T3, T4, T5, T6>(Joins, InternalDbQuery);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> selector)
        {
            return JoinSelect<TResult>(this, selector);
        }
    }

    internal class JoinQuery<T1, T2, T3, T4, T5, T6> : JoinQuery<T1, T2, T3, T4, T5>, IJoinQuery<T1, T2, T3, T4, T5, T6>
    {
        public JoinQuery(List<JoinQuery> joins, IInternalQuery dbQuery)
            : base(joins, dbQuery)
        {
        }

        public IJoinQuery<T1, T2, T3, T4, T5, T6, T7> Join<T7>(IQuery<T7> query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            Joins.Add(CreateJoinQuery(predicate, query.As<Query>().InternalQuery, joinType));
            return new JoinQuery<T1, T2, T3, T4, T5, T6, T7>(Joins, InternalDbQuery);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> selector)
        {
            return JoinSelect<TResult>(this, selector);
        }
    }

    internal class JoinQuery<T1, T2, T3, T4, T5, T6, T7> : JoinQuery<T1, T2, T3, T4, T5, T6>, IJoinQuery<T1, T2, T3, T4, T5, T6, T7>
    {
        public JoinQuery(List<JoinQuery> joins, IInternalQuery dbQuery)
            : base(joins, dbQuery)
        {
        }

        public IJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> Join<T8>(IQuery<T8> query, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate, JoinType joinType = JoinType.Inner)
        {
            Joins.Add(CreateJoinQuery(predicate, query.As<Query>().InternalQuery, joinType));
            return new JoinQuery<T1, T2, T3, T4, T5, T6, T7, T8>(Joins, InternalDbQuery);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> selector)
        {
            return JoinSelect<TResult>(this, selector);
        }
    }

    internal class JoinQuery<T1, T2, T3, T4, T5, T6, T7, T8> : JoinQuery<T1, T2, T3, T4, T5, T6, T7>, IJoinQuery<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public JoinQuery(List<JoinQuery> joins, IInternalQuery dbQuery)
            : base(joins, dbQuery)
        {
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> selector)
        {
            return JoinSelect<TResult>(this, selector);
        }
    }
}
