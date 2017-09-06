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
        public override Expression Translate(QueryContext context, MemberInitExpression memberInit, ExpressionVisitor transformVisitor)
        {
            var newExpression = transformVisitor.Visit(memberInit.NewExpression).Cast<NewExpression>();
            var newBindings = new List<MemberBinding>();

            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                var bindExpression = transformVisitor.Visit(binding.Expression);
                if (typeof(IEnumerable).IsAssignableFrom(binding.Member.GetMemberInfoType()))
                {
                    throw new NotImplementedException("not supported!");
                }

                context.EntityMemberMappingProvider.Map(binding.Member, bindExpression);
                newBindings.Add(Expression.Bind(binding.Member, bindExpression));
            }

            return memberInit.Update(newExpression, newBindings);
        }
    }
}
