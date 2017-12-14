using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class NewExpressionTranslator : Translator<NewExpression>
    {
        public static Dictionary<Expression, Expression> Maped = new Dictionary<Expression, Expression>();

        public NewExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(NewExpression newExpression)
        {
            if (newExpression.Arguments?.Count == 0)
            {
                return newExpression;
            }

            var arguments = new List<Expression>();
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                var argument = GetCompiledExpression(newExpression.Arguments[i]);
                //这里不应该返回 QueryExpression就是返回一个简单的Item
                //Item.Id 表示的Source.Id Source=QueryExpression
                if (argument.Is<QueryExpression>())
                {
                    arguments.Add(newExpression.Arguments[i]);
                    continue;
                }

                argument = new AliasExpression(Context.Alias.GetNewColumn(), argument, newExpression.Arguments[i]);
                arguments.Add(argument);
                Maped[newExpression.Arguments[i]] = argument;
            }

            return newExpression.Update(arguments);
        }

        //public override Expression Translate(NewExpression newExpression)
        //{
        //    if (newExpression.Arguments?.Count == 0)
        //    {
        //        return newExpression;
        //    }

        //    var arguments = new List<Expression>();
        //    for (var i = 0; i < newExpression.Arguments.Count; i++)
        //    {
        //        if (newExpression.Members == null)
        //        {
        //            continue;
        //        }

        //        var mem = newExpression.Members[i];
        //        if (mem == null)
        //        {
        //            continue;
        //        }

        //        var argument = GetCompiledExpression(newExpression.Arguments[i]);

        //        var col = argument.As<ColumnExpression>();
        //        if (col != null)
        //        {
        //            col.Alias = mem.Name;
        //        }

        //        argument = new AliasExpression(mem.Name, argument);

        //        argument.As<QueryExpression>()?.ChangeTypeOfExpression(mem.GetPropertyType());

        //        Maped[newExpression.Arguments[i]] = new AliasExpression(mem.Name, argument);

        //        arguments.Add(argument);
        //        Context.MemberAccessMapper.Map(mem, argument);
        //    }

        //    return newExpression.Update(arguments);
        //}
    }
}
