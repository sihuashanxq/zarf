using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers.NodeTypes
{
    public class NewNodeHandler : QueryNodeHandler<NewExpression>
    {
        public NewNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression HandleNode(NewExpression newExpression)
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
                    var query = QueryContext.SelectMapper.GetValue(argument);
                    if (query == null || !query.QueryModel.ContainsModel(newExpression))
                    {
                        if (!(argument is ColumnExpression col && !string.IsNullOrEmpty(col.Alias)))
                        {
                            argument = new AliasExpression(QueryContext.AliasGenerator.GetNewColumn(), argument, newExpression.Arguments[i]);
                        }
                    }

                    arguments.Add(argument);
                }

                QueryContext.BindingMaper.Map(Expression.MakeMemberAccess(newExpression, newExpression.Members[i]), argument);
            }

            return newExpression.Update(arguments);
        }
    }
}
