using System.Linq.Expressions;
using Zarf.Query.Expressions;
using Zarf.Extensions;
using System;
using System.Reflection;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Type EntityType { get; }

        public Expression EntityObject { get; }

        public MemberInfo Member { get; }

        public BindingContext(Type entityType, Expression entityObject, MemberInfo member = null)
        {
            EntityType = entityType;
            EntityObject = entityObject;
            Member = member;
        }

        internal static int GetExpressionOridinal(Expression node, QueryExpression queryExpression)
        {
            if (queryExpression == null || node == null)
            {
                return -1;
            }

            if (queryExpression.Projections.Count == 0 && queryExpression.SubQuery != null)
            {
                return GetExpressionOridinal(node, queryExpression.SubQuery);
            }

            var oridinal = -1;

            foreach (var item in queryExpression.Projections)
            {
                if (item == node)
                {
                    return oridinal;
                }

                if (node.Is<ColumnExpression>() && item.Is<ColumnExpression>())
                {
                    var column = item.As<ColumnExpression>();
                    var other = node.As<ColumnExpression>();

                    if (column?.Member == other?.Member)
                    {
                        return oridinal;
                    }
                }

                oridinal++;
            }

            return -1;
        }
    }
}
