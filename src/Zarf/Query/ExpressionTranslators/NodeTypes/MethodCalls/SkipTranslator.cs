using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class SkipTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SkipTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Skip");
        }

        public SkipTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression offSet, MethodInfo method)
        {
            select.Offset = new SkipExpression((int)offSet.As<ConstantExpression>().Value, select.Orders.ToList());
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }
    }
}
