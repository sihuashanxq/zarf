using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class WhereTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static WhereTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Where");
        }

        public WhereTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            if (query.Where != null && (query.Columns.Count != 0 || query.Sets.Count != 0))
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            RegisterQuerySource(GetFirstLambdaParameter(methodCall.Arguments[1]), query);

            var predicate = new RealtionExpressionVisitor().Visit(GetCompiledExpression(methodCall.Arguments[1]).UnWrap());
            if (methodCall.Method.Name == "SingleOrDefault")
            {
                query.DefaultIfEmpty = true;
            }

            query.AddWhere(predicate);
            return query;
        }
    }

    public class RealtionExpressionVisitor : ExpressionVisitors.ExpressionVisitorBase
    {
        public override Expression Visit(Expression node)
        {
            if (!node.Is<BinaryExpression>())
            {
                return base.Visit(node);
            }

            //NOT AND ÊÇ UnaryExpression
            var binary = node.As<BinaryExpression>();
            if (binary.Left.NodeType != ExpressionType.Extension && binary.Right.NodeType != ExpressionType.Extension)
            {
                return base.Visit(binary);
            }

            //QueryExpression AggrateExpression AllExpression AnyExpression
            switch (binary.NodeType)
            {
                case ExpressionType.Equal:
                    return Expression.Not(VisitNotEqual(binary.Left, binary.Right));
                case ExpressionType.NotEqual:
                    return VisitNotEqual(binary.Left, binary.Right);
                case ExpressionType.And:
                    break;
                case ExpressionType.AndAlso:
                    break;
                case ExpressionType.GreaterThan:
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    break;
                case ExpressionType.LessThan:
                    break;
                case ExpressionType.LessThanOrEqual:
                    break;
                case ExpressionType.Not:
                    return VisitNot(binary.Left, binary.Right);
                case ExpressionType.Or:
                    break;
                case ExpressionType.OrElse:
                    break;
            }

            throw new System.Exception();
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            var lambdaBody = Visit(lambda.Body);
            if (lambdaBody != lambda.Body)
            {
                return Expression.Lambda(lambdaBody, lambda.Parameters);
            }

            return lambda;
        }

        protected virtual Expression VisitNotEqual(Expression left, Expression right)
        {
            if (left.Is<QueryExpression>() && right.IsNullValueConstant())
            {
                return new ExistsExpression(left.As<QueryExpression>());
            }

            if (right.Is<QueryExpression>() && left.IsNullValueConstant())
            {
                return new ExistsExpression(right.As<QueryExpression>());
            }

            return null;
        }

        protected virtual Expression VisitNot(Expression oprand)
        {
            if (oprand.Is<AnyExpression>() || oprand.Is<AllExpression>())
            {

            }
            return null;
        }
    }
}