using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;

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
                var bindExpression = Compiler.Compile(binding.Expression);
                var memberInfoType = binding.Member.GetPropertyType();

                if (typeof(IEnumerable).IsAssignableFrom(memberInfoType) && memberInfoType != typeof(string))
                {
                    throw new NotImplementedException("not supported!");
                }

                Context.MemberAccessMapper.Map(binding.Member, bindExpression);
                bindings.Add(Expression.Bind(binding.Member, bindExpression));
            }

            return memInit.Update(newExpression, bindings);
        }
    }
}
