using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class TakeTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static TakeTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Take");
        }

        public TakeTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression limit, MethodInfo method)
        {
            select.Limit = Convert.ToInt32(limit.As<ConstantExpression>().Value);
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }
    }
}
