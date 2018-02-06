using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Core.Internals;
using Zarf.Query.Expressions;
using Zarf.Query.Visitors;

namespace Zarf.Query.Handlers.NodeTypes.MethodCalls
{
    public class JoinNodeHandler : MethodNodeHandler
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        protected List<SelectExpression> Selects { get; set; }

        static JoinNodeHandler()
        {
            SupprotedMethods = new[] { ReflectionUtil.JoinMethod };
        }

        public JoinNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public virtual Expression Transalte(JoinQuery joinQuery)
        {
            return HandleNode(
                Compile<SelectExpression>(joinQuery.InternalDbQuery.Expression),
                Expression.Constant(joinQuery.Joins),
                null);
        }

        public override SelectExpression HandleNode(SelectExpression select, Expression expression, MethodInfo method)
        {
            var joins = Compile<ConstantExpression>(expression)?.Value as List<JoinQuery>;
            if (joins == null)
            {
                throw new Exception("error join query!");
            }

            if (select.Sets.Count > 0)
            {
                select = select.PushDownSubQuery(QueryContext.AliasGenerator.GetNewTable());
            }

            Selects = new List<SelectExpression> { select };

            foreach (var item in joins)
            {
                Selects.Add(GetJoinQuery(item.InternalDbQuery));
                Selects.LastOrDefault().Projections.Clear();

                select.AddJoin(new JoinExpression(Selects.Last(), null, item.JoinType));

                for (var i = 0; i < item.Predicate.Parameters.Count; i++)
                {
                    QueryContext.SelectMapper.Map(item.Predicate.Parameters[i], Selects[i]);
                    QueryContext.ModelMapper.Map(item.Predicate.Parameters[i], Selects[i].QueryModel);
                }

                select.Joins.LastOrDefault().Predicate = HandlePredicate(select, item.Predicate);
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

        protected Expression HandlePredicate(SelectExpression select, Expression predicate)
        {
            predicate = new RelationExpressionVisitor(QueryContext).Compile(predicate);
            predicate = new RelationExpressionConvertVisitor().Visit(predicate);
            predicate = new QueryModelRewriterExpressionVisitor(select, QueryContext).ChangeQueryModel(predicate);

            return predicate;
        }
    }
}
