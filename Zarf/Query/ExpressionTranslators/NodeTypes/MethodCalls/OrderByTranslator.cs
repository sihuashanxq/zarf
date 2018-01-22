using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Zarf.Extensions;
using Zarf.Metadata.Entities;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class OrderByTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static OrderByTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods
                .Where(
                    item =>
                        item.Name == "ThenBy" ||
                        item.Name == "OrderBy" ||
                        item.Name == "ThenByDescending" ||
                        item.Name == "OrderByDescending"
                );
        }

        public OrderByTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression keySelector, MethodInfo method)
        {
            var parameter = keySelector.GetParameters().FirstOrDefault();
            if (select.Sets.Count != 0)
            {
                select = select.PushDownSubQuery(QueryContext.AliasGenerator.GetNewTable());
            }

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);

            var key = new RelationExpressionCompiler(QueryContext).Visit(keySelector.UnWrap().As<LambdaExpression>().Body);
            if (key is AliasExpression alias)
            {
                var keySelect = QueryContext.SelectMapper.GetValue(alias);
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

            var cols = new[] { key.As<ColumnExpression>() };
            var direction = method.Name.EndsWith("Descending") ? OrderDirection.Desc : OrderDirection.Asc;

            select.Orders.Add(new OrderExpression(cols, direction));

            return select;
        }
    }
}
