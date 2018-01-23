using System;
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
                select = select.PushDownSubQuery(QueryContext.AliasGenerator.GetNewTable());
            }

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);

            var groupKey = GetGroupKey(select, keySelector);
            if (groupKey == null)
            {
                throw new Exception("not found group by field!");
            }

            select.Groups.Add(new GroupExpression(new[] { groupKey }));

            return select;
        }

        protected virtual ColumnExpression GetGroupKey(SelectExpression select, Expression keySelector)
        {
            var groupKey = new RelationExpressionVisitor(QueryContext).Visit(keySelector.UnWrap().As<LambdaExpression>().Body);
            if (groupKey is AliasExpression alias)
            {
                var rSelect = QueryContext.SelectMapper.GetValue(alias);
                if (!select.ContainsSelectExpression(rSelect))
                {
                    throw new Exception("group by field must in current select!");
                }

                if (alias.Expression is ColumnExpression)
                {
                    return alias.Expression.As<ColumnExpression>();
                }

                if (rSelect != select)
                {
                    return new ColumnExpression(rSelect, new Column(alias.Alias), alias.Type);
                }
            }

            return groupKey as ColumnExpression;
        }
    }
}
