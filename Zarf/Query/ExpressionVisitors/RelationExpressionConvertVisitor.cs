using System;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    /// <summary>
    /// 查询条件转换
    /// </summary>
    public class RelationExpressionConvertVisitor : ExpressionVisitorBase
    {
        public override Expression Visit(Expression node)
        {
            if (node.Is<AllExpression>())
            {
                return VisitAll(node.As<AllExpression>());
            }

            if (node.Is<AnyExpression>())
            {
                return VisitAny(node.As<AnyExpression>());
            }

            if (node.Is<SelectExpression>())
            {
                node.As<SelectExpression>().IsInPredicate = true;
            }

            if (node is AliasExpression alias)
            {
                return alias.Expression;
            }

            if (node.NodeType == ExpressionType.Extension)
            {
                return node;
            }

            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    return VisitEqual(node.As<BinaryExpression>());
                case ExpressionType.NotEqual:
                    return VisitNotEqual(node.As<BinaryExpression>());
                case ExpressionType.Or:
                    return VisitOr(node.As<BinaryExpression>());
                case ExpressionType.And:
                    return VisitAnd(node.As<BinaryExpression>());
            }

            return base.Visit(node);
        }

        protected virtual Expression VisitEqual(BinaryExpression binary)
        {
            var select = null as SelectExpression;
            if (binary.Left.Is<SelectExpression>() && binary.Right.IsNullValueConstant())
            {
                select = binary.Left.As<SelectExpression>();
            }
            else if (binary.Right.Is<SelectExpression>() && binary.Left.IsNullValueConstant())
            {
                select = binary.Right.As<SelectExpression>();
            }

            if (select != null)
            {
                select.Where = new WhereExperssion(Visit(select.Where.Predicate));
                return Expression.Not(new ExistsExpression(select));
            }

            if (!binary.Left.Type.IsPrimtiveType())
            {
                throw new NotSupportedException($"not supported compared the value of {binary.Left.Type.Name}");
            }

            return base.Visit(binary);
        }

        protected virtual Expression VisitNotEqual(BinaryExpression binary)
        {
            var select = null as SelectExpression;
            if (binary.Left.Is<SelectExpression>() && binary.Right.IsNullValueConstant())
            {
                select = binary.Left.As<SelectExpression>();
            }
            else if (binary.Right.Is<SelectExpression>() && binary.Left.IsNullValueConstant())
            {
                select = binary.Right.As<SelectExpression>();
            }

            if (select != null)
            {
                select.Where = new WhereExperssion(Visit(select.Where.Predicate));
                return new ExistsExpression(select);
            }

            if (!ReflectionUtil.SimpleTypes.Contains(binary.Left.Type))
            {
                throw new System.NotSupportedException($"not supported compared the value of {binary.Left.Type.Name}");
            }

            return base.Visit(binary);
        }

        protected virtual Expression VisitOr(BinaryExpression binary)
        {
            if (binary.Left.Type != typeof(bool))
            {
                return base.Visit(binary);
            }

            var left = Visit(binary.Left);
            var right = Visit(binary.Right);
            if (!left.Is<ExistsExpression>() && !left.Is<BinaryExpression>())
            {
                left = Expression.Equal(left, Expression.Constant(true));
            }

            if (!right.Is<ExistsExpression>() && !right.Is<BinaryExpression>())
            {
                right = Expression.Equal(right, Expression.Constant(true));
            }

            return Expression.OrElse(left, right);
        }

        protected virtual Expression VisitAnd(BinaryExpression binary)
        {
            if (binary.Left.Type != typeof(bool))
            {
                return base.Visit(binary);
            }

            var left = Visit(binary.Left);
            var right = Visit(binary.Right);
            if (!left.Is<ExistsExpression>() && !left.Is<BinaryExpression>())
            {
                left = Expression.Equal(left, Expression.Constant(true));
            }

            if (!right.Is<ExistsExpression>() && !right.Is<BinaryExpression>())
            {
                right = Expression.Equal(right, Expression.Constant(true));
            }

            return Expression.AndAlso(left, right);
        }

        protected virtual Expression VisitAll(AllExpression all)
        {
            all.Select.Where = new WhereExperssion(Visit(all.Select.Where.Predicate));
            return Expression.Not(new ExistsExpression(all.Select));
        }

        protected virtual Expression VisitAny(AnyExpression any)
        {
            any.Select.Where = new WhereExperssion(Visit(any.Select.Where.Predicate));
            return new ExistsExpression(any.Select);
        }
    }
}
