using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            var newExpression = GetCompiledExpression<NewExpression>(memInit.NewExpression);
            var bindings = new List<MemberBinding>();

            foreach (var binding in memInit.Bindings.OfType<MemberAssignment>())
            {
                var bindExpression = GetCompiledExpression(binding.Expression);
                if (bindExpression.Is<QueryExpression>())
                {
                    bindExpression = binding.Expression;
                }
                else
                {
                    bindExpression = new AliasExpression(Context.Alias.GetNewColumn(), bindExpression, binding.Expression);
                }

                Context.MemberBindingMapper.Map(Expression.MakeMemberAccess(newExpression, binding.Member), bindExpression);
                bindings.Add(Expression.Bind(binding.Member, bindExpression));
            }

            return memInit.Update(newExpression, bindings);
        }
    }
}
