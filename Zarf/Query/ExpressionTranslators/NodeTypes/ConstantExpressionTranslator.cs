using System;
using System.Linq.Expressions;
using Zarf.Core.Internals;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    class ConstantExpressionTranslator : Translator<ConstantExpression>
    {
        public ConstantExpressionTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(ConstantExpression constant)
        {
            if (!typeof(IInternalQuery).IsAssignableFrom(constant.Type))
            {
                return constant;
            }

            var typeOfEntity = constant.Type.GenericTypeArguments?[0];
            if (typeOfEntity == null)
            {
                throw new NotImplementedException("using IDataQuery<T>");
            }

            return new QueryExpression(typeOfEntity, Context.ColumnCaching, Context.Alias.GetNewTable());
        }
    }
}
