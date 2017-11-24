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
        public override Expression Translate(IQueryContext context, NewExpression newExpression, IQueryCompiler queryCompiler)
        {
            if (newExpression.Arguments == null ||
                newExpression.Arguments.Count == 0)
            {
                return newExpression;
            }

            var arguments = new List<Expression>();
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                var argument = queryCompiler.Compile(newExpression.Arguments[i]);
                if (newExpression.Members == null || newExpression.Members[i] == null)
                {
                    continue;
                }

                if (argument is ColumnExpression)
                {
                    argument.Cast<ColumnExpression>().Alias = newExpression.Members[i].Name;
                }
                else if (argument is QueryExpression && newExpression.Members[i].GetPropertyType().IsCollection())
                {
                    throw new NotImplementedException("not supported!");
                }

                context.EntityMemberMappingProvider.Map(newExpression.Members[i], argument);
                arguments.Add(argument);
            }

            return newExpression.Update(arguments);
        }
    }
}
