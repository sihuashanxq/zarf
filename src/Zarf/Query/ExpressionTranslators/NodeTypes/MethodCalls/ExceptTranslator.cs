using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class ExceptTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static ExceptTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Except");
        }

        public ExceptTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {
        }

        public override SelectExpression Translate(SelectExpression select, Expression sets, MethodInfo method)
        {
            var setsSelect = Compile<SelectExpression>(sets);

            Utils.CheckNull(select, "Query Expression");
            Utils.CheckNull(setsSelect, "Except Query Expression");

            select.Sets.Add(new ExceptExpression(setsSelect));
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }
    }
}
