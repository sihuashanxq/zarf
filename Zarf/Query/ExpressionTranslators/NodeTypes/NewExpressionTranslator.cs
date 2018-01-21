using System.Collections.Generic;
using System.Linq.Expressions;

using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class NewExpressionTranslator : Translator<NewExpression>
    {
        public NewExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(NewExpression newExpression)
        {
            if (newExpression.Arguments.Count == 0)
            {
                return newExpression;
            }

            var arguments = new List<Expression>();
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                var argument = Compile(newExpression.Arguments[i]);
                if (argument.Is<SelectExpression>())
                {
                    arguments.Add(newExpression.Arguments[i]);
                }
                else
                {
                    var query = QueryContext.ProjectionOwner.GetSelectExpression(argument);
                    if (query == null || !query.QueryModel.ContainsModel(newExpression))
                    {
                        argument = new AliasExpression(QueryContext.Alias.GetNewColumn(), argument, newExpression.Arguments[i]);
                    }

                    arguments.Add(argument);
                }

                QueryContext.MemberBindingMapper.Map(Expression.MakeMemberAccess(newExpression, newExpression.Members[i]), argument);
            }

            return newExpression.Update(arguments);
        }
    }
}
