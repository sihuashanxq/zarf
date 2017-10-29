using System;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Query.Expressions;
using System.Reflection;

namespace Zarf.Query.ExpressionTranslators.NodeTypes
{
    class ConstantExpressionTranslator : Translator<ConstantExpression>
    {
        public override Expression Translate(IQueryContext context, ConstantExpression constant, IQueryCompiler queryCompiler)
        {
            if (!typeof(IDbQuery).IsAssignableFrom(constant.Type))
            {
                return constant;
            }

            var entityType = constant.Type.GenericTypeArguments.FirstOrDefault();
            if (entityType == null)
            {
                throw new NotImplementedException("using IDataQuery<T>");
            }

            return new QueryExpression(entityType, context.Alias.GetNewTable());
        }
    }
}
