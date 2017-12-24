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
            var joins = methodCall.Arguments[1].As<ConstantExpression>()?.Value as List<JoinQuery>;
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
                var joinQuery = GetJoinQuery(item.InternalDbQuery);

                for (var i = 0; i < item.Predicate.Parameters.Count; i++)
                {
                    Context.QueryMapper.MapQuery(item.Predicate.Parameters[i], Queries[i]);

                    if (Queries[i].QueryModel != null)
                    {
                        Context.QueryModelMapper.MapQueryModel(item.Predicate.Parameters[i], Queries[i].QueryModel);
                    }
                }

                var predicate = CreateRealtionCompiler(query).Compile(item.Predicate);
                var join = new JoinExpression(Queries.Last(), predicate, item.JoinType);
                query.AddJoin(join);
            }

            return query;
        }

        private QueryExpression GetJoinQuery(IInternalQuery dbQuery)
        {
            var expression = dbQuery.GetType().GetProperty("Expression").GetValue(dbQuery) as Expression;
            if (expression == null)
            {
                throw new Exception("error join query!");
            }

            var joinQuery = GetCompiledExpression<QueryExpression>(expression);

            Queries.Add(joinQuery);

            return joinQuery;
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(QueryExpression query)
        {
            return new RelationExpressionCompiler(Context);
        }
    }
}
