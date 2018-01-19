using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Queries.Expressions;
using Zarf.Queries.ExpressionVisitors;

namespace Zarf.Queries.ExpressionTranslators.Methods
{
    public class GroupByTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static GroupByTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "GroupBy");
        }

        public GroupByTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            return Translate(query, methodCall.Arguments[1]);
        }

        public virtual QueryExpression Translate(QueryExpression query, Expression keySelector)
        {
            var parameter = keySelector.GetParameters().FirstOrDefault();
            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            Context.QueryMapper.MapQuery(parameter, query);
            Context.QueryModelMapper.MapQueryModel(parameter, query.QueryModel);

            var keyExpression = new RelationExpressionCompiler(Context).Visit(keySelector.UnWrap().As<LambdaExpression>().Body);
            if (keyExpression is AliasExpression alias)
            {
                var keyQuery = Context.ProjectionOwner.GetQuery(alias);
                if (!query.ConstainsQuery(keyQuery))
                {
                    throw new System.Exception("");
                }
                else if (alias.Expression is ColumnExpression)
                {
                    keyExpression = alias.Expression.As<ColumnExpression>();
                }
                else if (keyQuery != query)
                {
                    keyExpression = new ColumnExpression(keyQuery, new Column(alias.Alias), alias.Type);
                }
            }

            if (!keyExpression.Is<ColumnExpression>())
            {
                throw new System.Exception();
            }

            query.Groups.Add(new GroupExpression(new[] { keyExpression.As<ColumnExpression>() }));

            return query;
        }
    }
}
