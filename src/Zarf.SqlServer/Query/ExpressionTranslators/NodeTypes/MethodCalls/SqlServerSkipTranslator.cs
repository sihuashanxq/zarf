using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Metadata.Entities;
using Zarf.Query;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls;

namespace Zarf.SqlServer.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class SqlServerSkipTranslator : SkipTranslator
    {
        public SqlServerSkipTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression offSet, MethodInfo method)
        {
            var offset = (int)offSet.As<ConstantExpression>().Value;
            var skip = new SkipExpression(offset, select.Orders.ToList());

            Utils.CheckNull(select, "query");

            select.AddProjection(skip);
            select = select.PushDownSubQuery(QueryContext.AliasGenerator.GetNewTable());

            var skipColumn = new ColumnExpression(select, new Column("__rowIndex__"), typeof(int));
            var skipCondtion = Expression.MakeBinary(
                ExpressionType.GreaterThan,
                skipColumn,
                Expression.Constant(offset));

            select.CombineCondtion(Expression.Lambda(skipCondtion));
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }
    }
}
