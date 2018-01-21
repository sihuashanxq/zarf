using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class GroupByTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static GroupByTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "GroupBy");
        }

        public GroupByTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression keySelector, MethodInfo method)
        {
            var parameter = keySelector.GetParameters().FirstOrDefault();
            if (select.Sets.Count != 0)
            {
                select = select.PushDownSubQuery(QueryContext.Alias.GetNewTable());
            }

            QueryContext.QueryMapper.AddSelectExpression(parameter, select);
            QueryContext.QueryModelMapper.MapQueryModel(parameter, select.QueryModel);

            var key = new RelationExpressionCompiler(QueryContext).Visit(keySelector.UnWrap().As<LambdaExpression>().Body);
            if (key is AliasExpression alias)
            {
                var keySelect = QueryContext.ProjectionOwner.GetSelectExpression(alias);

                if (!select.ContainsSelectExpression(keySelect))
                {
                    throw new System.Exception("");
                }

                if (alias.Expression is ColumnExpression)
                {
                    key = alias.Expression.As<ColumnExpression>();
                }
                else if (keySelect != select)
                {
                    key = new ColumnExpression(keySelect, new Column(alias.Alias), alias.Type);
                }
            }

            if (!key.Is<ColumnExpression>())
            {
                throw new System.Exception();
            }

            select.Groups.Add(new GroupExpression(new[] { key.As<ColumnExpression>() }));

            return select;
        }
    }
}
