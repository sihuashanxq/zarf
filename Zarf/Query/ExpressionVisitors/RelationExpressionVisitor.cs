using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core.Internals;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    /// <summary>
    /// 查询条件转换
    /// </summary>
    public class RelationExpressionCompiler : QueryCompiler
    {
        public RelationExpressionCompiler(IQueryContext context) : base(context)
        {

        }

        public override Expression Compile(Expression query)
        {
            if (query == null)
            {
                return query;
            }

            if (query.NodeType == ExpressionType.MemberAccess)
            {
                return VisitMember(query.As<MemberExpression>());
            }

            return base.Compile(query);
        }

        protected override Expression VisitMember(MemberExpression mem)
        {
            var queryModel = Context.QueryModelMapper.GetQueryModel(mem.Expression);
            if (queryModel != null)
            {
                var property = Expression.MakeMemberAccess(queryModel.Model, mem.Member);
                var propertyExpression = Context.MemberBindingMapper.GetMapedExpression(property);
                if (propertyExpression.NodeType != ExpressionType.Extension)
                {
                    propertyExpression = base.Visit(propertyExpression);
                }

                if (propertyExpression.Is<AliasExpression>())
                {
                    return propertyExpression.As<AliasExpression>().Expression;
                }

                return propertyExpression;
            }

            return base.Compile(mem);
        }
    }

    public class RelationExpressionVisitor : ExpressionVisitorBase
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

            if (node.Is<QueryExpression>())
            {
                node.As<QueryExpression>().IsPartOfPredicate = true;
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
            var query = null as QueryExpression;
            if (binary.Left.Is<QueryExpression>() && binary.Right.IsNullValueConstant())
            {
                query = binary.Left.As<QueryExpression>();
            }
            else if (binary.Right.Is<QueryExpression>() && binary.Left.IsNullValueConstant())
            {
                query = binary.Right.As<QueryExpression>();
            }

            if (query != null)
            {
                query.Where = new WhereExperssion(Visit(query.Where.Predicate));
                return Expression.Not(new ExistsExpression(query));
            }

            if (!binary.Left.Type.IsPrimtiveType())
            {
                throw new System.NotSupportedException($"not supported compared the value of {binary.Left.Type.Name}");
            }

            return base.Visit(binary);
        }

        protected virtual Expression VisitNotEqual(BinaryExpression binary)
        {
            var query = null as QueryExpression;
            if (binary.Left.Is<QueryExpression>() && binary.Right.IsNullValueConstant())
            {
                query = binary.Left.As<QueryExpression>();
            }
            else if (binary.Right.Is<QueryExpression>() && binary.Left.IsNullValueConstant())
            {
                query = binary.Right.As<QueryExpression>();
            }

            if (query != null)
            {
                query.Where = new WhereExperssion(Visit(query.Where.Predicate));
                return new ExistsExpression(query);
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
            all.Query.Where = new WhereExperssion(Visit(all.Query.Where.Predicate));
            return Expression.Not(new ExistsExpression(all.Query));
        }

        protected virtual Expression VisitAny(AnyExpression any)
        {
            any.Query.Where = new WhereExperssion(Visit(any.Query.Where.Predicate));
            return new ExistsExpression(any.Query);
        }
    }
}
