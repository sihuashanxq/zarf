using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    public class MemberInitExpressionTranslator : Translator<MemberInitExpression>
    {
        public MemberInitExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override Expression Translate(MemberInitExpression memInit)
        {
            var newExpression = Compile<NewExpression>(memInit.NewExpression);
            var bindings = new List<MemberBinding>();

            foreach (var binding in memInit.Bindings.OfType<MemberAssignment>())
            {
                var bindExpression = Compile(binding.Expression);
                if (bindExpression.Is<SelectExpression>())
                {
                    bindExpression = binding.Expression;
                }
                else
                {
                    var query = QueryContext.SelectMapper.GetValue(bindExpression);
                    if (query == null || !query.QueryModel.ContainsModel(memInit))
                    {
                        bindExpression = new AliasExpression(QueryContext.AliasGenerator.GetNewColumn(), bindExpression, binding.Expression);
                    }
                }

                QueryContext.BindingMaper.Map(Expression.MakeMemberAccess(memInit, binding.Member), bindExpression);
                bindings.Add(Expression.Bind(binding.Member, bindExpression));
            }

            return memInit.Update(newExpression, bindings);
        }
    }
}
