using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class IntersectTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static IntersectTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Intersect");
        }

        public IntersectTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression sets,MethodInfo method)
        {
            var setsSelect= Compile<SelectExpression>(sets);

            Utils.CheckNull(select, "Query Expression");
            Utils.CheckNull(setsSelect, "Except Query Expression");

            select.Sets.Add(new IntersectExpression(setsSelect));

            return select;
        }
    }
}
