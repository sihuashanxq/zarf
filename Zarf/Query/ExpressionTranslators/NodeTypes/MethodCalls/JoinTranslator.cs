using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class JoinTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        public List<QueryExpression> Queries { get; }

        static JoinTranslator()
        {
            SupprotedMethods = new[] { ReflectionUtil.Join };
        }

        public JoinTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
            Queries = new List<QueryExpression>();
        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var joins = GetCompiledExpression<ConstantExpression>(methodCall.Arguments[1])?.Value as List<JoinQuery>;
            if (joins == null)
            {
                throw new Exception("error join query!");
            }

            if (query.Where != null)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            Queries.Add(query);

            foreach (var item in joins)
            {
                Queries.Add(GetJoinQuery(item.InternalDbQuery));
                Queries.LastOrDefault().Projections.Clear();

                query.AddJoin(new JoinExpression(Queries.Last(), null, item.JoinType));

                for (var i = 0; i < item.Predicate.Parameters.Count; i++)
                {
                    Context.QueryMapper.MapQuery(item.Predicate.Parameters[i], Queries[i]);
                    Context.QueryModelMapper.MapQueryModel(item.Predicate.Parameters[i], Queries[i].QueryModel);
                }

                var predicate = CreateRealtionCompiler(query).Compile(item.Predicate);
                predicate = new RelationExpressionVisitor().Visit(predicate);
                predicate = new SubQueryModelRewriter(query, Context).ChangeQueryModel(predicate);

                query.Joins.LastOrDefault().Predicate = predicate;
            }

            return query;
        }

        public virtual Expression Transalte(JoinQuery joinQuery)
        {
            var query = GetCompiledExpression<QueryExpression>(joinQuery.InternalDbQuery.GetExpression());
            var joins = joinQuery.Joins;
            if (joins == null)
            {
                throw new Exception("error join query!");
            }

            if (query.Where != null)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            Queries.Add(query);

            foreach (var item in joins)
            {
                Queries.Add(GetJoinQuery(item.InternalDbQuery));
                Queries.LastOrDefault().Projections.Clear();

                query.AddJoin(new JoinExpression(Queries.Last(), null, item.JoinType));

                for (var i = 0; i < item.Predicate.Parameters.Count; i++)
                {
                    Context.QueryMapper.MapQuery(item.Predicate.Parameters[i], Queries[i]);
                    Context.QueryModelMapper.MapQueryModel(item.Predicate.Parameters[i], Queries[i].QueryModel);
                }

                var predicate = CreateRealtionCompiler(query).Compile(item.Predicate);
                predicate = new RelationExpressionVisitor().Visit(predicate);
                predicate = new SubQueryModelRewriter(query, Context).ChangeQueryModel(predicate);

                query.Joins.LastOrDefault().Predicate = predicate;
            }

            return query;
        }

        protected virtual QueryExpression GetJoinQuery(IInternalQuery dbQuery)
        {
            var expression = dbQuery.GetType().GetProperty("Expression").GetValue(dbQuery) as Expression;
            if (expression == null)
            {
                throw new Exception("error join query!");
            }

            return GetCompiledExpression<QueryExpression>(expression);
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(QueryExpression query)
        {
            return new RelationExpressionCompiler(Context);
        }
    }
}
