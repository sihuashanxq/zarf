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
        public NewExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper)
            : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(NewExpression newExp)
        {
            if (newExp.Arguments?.Count == 0)
            {
                return newExp;
            }

            var args = new List<Expression>();
            for (var i = 0; i < newExp.Arguments.Count; i++)
            {
                var mem = newExp.Members?[i];
                if (mem == null)
                {
                    continue;
                }

                var arg = GetCompiledExpression(newExp.Arguments[i]);
                var col = arg.As<ColumnExpression>();
                if (col != null)
                {
                    col.Alias = mem.Name;
                }

                if (arg is QueryExpression && mem.GetPropertyType().IsCollection())
                {
                    throw new NotImplementedException("not supported!");
                }

                Context.EntityMemberMappingProvider.Map(mem, arg);
                args.Add(arg);
            }

            return newExp.Update(args);
        }
    }
}
