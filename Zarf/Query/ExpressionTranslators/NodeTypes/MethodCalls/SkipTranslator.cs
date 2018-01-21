using System.Collections.Generic;
using System.Linq.Expressions;

using Zarf.Extensions;
using Zarf.Query.Expressions;
using System.Linq;
using System.Reflection;
using Zarf.Metadata.Entities;

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
            var offset = (int)offSet.As<ConstantExpression>().Value;
            var skip = new SkipExpression(offset, select.Orders.ToList());

            Utils.CheckNull(select, "query");

            select.AddProjection(skip);
            select = select.PushDownSubQuery(QueryContext.Alias.GetNewTable());

            var skipColumn = new ColumnExpression(select, new Column("__rowIndex__"), typeof(int));
            var skipCondtion = Expression.MakeBinary(
                ExpressionType.GreaterThan,
                skipColumn,
                Expression.Constant(offset));

            select.CombineCondtion(Expression.Lambda(skipCondtion));

            return select;
        }
    }
}
