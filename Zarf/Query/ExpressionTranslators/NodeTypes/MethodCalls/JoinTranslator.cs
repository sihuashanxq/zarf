using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class JoinTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        protected List<SelectExpression> Selects { get; set; }

        static JoinTranslator()
        {
            SupprotedMethods = new[] { ReflectionUtil.JoinMethod };
        }

        public JoinTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public virtual Expression Transalte(JoinQuery joinQuery)
        {
            return Translate(
                Compile<SelectExpression>(joinQuery.InternalDbQuery.Expression),
                Expression.Constant(joinQuery.Joins),
                null);
        }

        public override SelectExpression Translate(SelectExpression select, Expression expression, MethodInfo method)
        {
            var joins = Compile<ConstantExpression>(expression)?.Value as List<JoinQuery>;
            if (joins == null)
            {
                throw new Exception("error join query!");
            }

            if (select.Where != null)
            {
                select = select.PushDownSubQuery(QueryContext.Alias.GetNewTable());
            }

            Selects = new List<SelectExpression> { select };

            foreach (var item in joins)
            {
                Selects.Add(GetJoinQuery(item.InternalDbQuery));
                Selects.LastOrDefault().Projections.Clear();

                select.AddJoin(new JoinExpression(Selects.Last(), null, item.JoinType));

                for (var i = 0; i < item.Predicate.Parameters.Count; i++)
                {
                    QueryContext.QueryMapper.AddSelectExpression(item.Predicate.Parameters[i], Selects[i]);
                    QueryContext.QueryModelMapper.MapQueryModel(item.Predicate.Parameters[i], Selects[i].QueryModel);
                }

                var predicate = CreateRealtionCompiler(select).Compile(item.Predicate);
                predicate = new RelationExpressionVisitor().Visit(predicate);
                predicate = new SubQueryModelRewriter(select, QueryContext).ChangeQueryModel(predicate);

                select.Joins.LastOrDefault().Predicate = predicate;
            }

            return select;
        }

        protected virtual SelectExpression GetJoinQuery(IInternalQuery query)
        {
            var expression = query.GetType().GetProperty("Expression").GetValue(query) as Expression;
            if (expression == null)
            {
                throw new Exception("error join query!");
            }

            return Compile<SelectExpression>(expression);
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(SelectExpression select)
        {
            return new RelationExpressionCompiler(QueryContext);
        }
    }
}
