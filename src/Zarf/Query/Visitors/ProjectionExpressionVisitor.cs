using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using System.Reflection;
using Zarf.Metadata.DataAnnotations;

namespace Zarf.Query.Visitors
{
    /// <summary>
    /// 生成查询投影
    /// </summary>
    public class ProjectionExpressionVisitor : QueryExpressionVisitor
    {
        public SelectExpression Select { get; }

        public ProjectionExpressionVisitor(SelectExpression select, IQueryContext queryContext)
            : base(queryContext)
        {
            Select = select;
            Select.Projections.Clear();
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInit)
        {
            VisitNew(memberInit.NewExpression);

            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                if (binding.Expression.Is<SelectExpression>())
                {
                    continue;
                }

                if (binding.Expression.NodeType == ExpressionType.Parameter)
                {
                    VisitParameter(binding.Expression.As<ParameterExpression>());
                    continue;
                }

                if (!ReflectionUtil.SimpleTypes.Contains(binding.Expression.Type))
                {
                    continue;
                }

                if (binding.Expression.NodeType != ExpressionType.Extension &&
                    binding.Expression.NodeType != ExpressionType.Constant)
                {
                    continue;
                }

                Select.AddProjection(binding.Expression);
                QueryContext.SelectMapper.Map(binding.Expression, Select);
                QueryContext.BindingMaper.Map(Expression.MakeMemberAccess(memberInit, binding.Member), binding.Expression);
            }

            return memberInit;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                if (newExpression.Arguments[i].Is<SelectExpression>())
                {
                    continue;
                }

                if (newExpression.Arguments[i].NodeType == ExpressionType.Parameter)
                {
                    VisitParameter(newExpression.Arguments[i].As<ParameterExpression>());
                    continue;
                }

                if (!ReflectionUtil.SimpleTypes.Contains(newExpression.Arguments[i].Type))
                {
                    continue;
                }

                if (newExpression.Arguments[i].NodeType != ExpressionType.Extension &&
                    newExpression.Arguments[i].NodeType != ExpressionType.Constant)
                {
                    continue;
                }

                Select.AddProjection(newExpression.Arguments[i]);
                QueryContext.SelectMapper.Map(newExpression.Arguments[i], Select);
                QueryContext.BindingMaper.Map(Expression.MakeMemberAccess(newExpression, newExpression.Members[i]), newExpression.Arguments[i]);
            }

            return newExpression;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            var query = QueryContext.SelectMapper.GetValue(parameter);
            if (query == null)
            {
                return parameter;
            }

            foreach (var item in query.GenSelectProjections())
            {
                Select.AddProjection(item);
                QueryContext.SelectMapper.Map(item, Select);
            }

            return parameter;
        }

        public override Expression Visit(Expression node)
        {
            var visitedNode = base.Visit(node);

            if (visitedNode.NodeType == ExpressionType.New)
            {
                return VisitNew(visitedNode.As<NewExpression>());
            }

            if (visitedNode.NodeType == ExpressionType.MemberInit)
            {
                return VisitMemberInit(visitedNode.As<MemberInitExpression>());
            }

            if (visitedNode is ColumnExpression ||
                visitedNode is AggregateExpression ||
                visitedNode is AliasExpression)
            {
                Select.AddProjection(visitedNode);
            }

            else if (visitedNode is CaseWhenExpression)
            {
                Select.AddProjection(visitedNode);
                QueryContext.ExpressionMapper.Map(node, visitedNode);
            }

            else if (visitedNode.Is<MethodCallExpression>() &&
                 IsSQLFunction(visitedNode.As<MethodCallExpression>()))
            {
                Select.AddProjection(visitedNode);
                QueryContext.ExpressionMapper.Map(node, visitedNode);
            }

            return visitedNode;
        }

        protected virtual bool IsSQLFunction(MethodCallExpression methodCall)
        {
            if (methodCall.Method.GetCustomAttribute<SQLFunctionHandlerAttribute>() != null)
            {
                return true;
            }

            return methodCall.Method.DeclaringType.IsPrimtiveType();
        }
    }
}
