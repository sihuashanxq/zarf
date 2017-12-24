using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    /// <summary>
    /// 根据子查询生成Column
    /// </summary>
    public class ProjectionExpressionVisitor : QueryCompiler
    {
        public QueryExpression Query { get; }

        public ProjectionExpressionVisitor(QueryExpression query, IQueryContext queryContext)
            : base(queryContext)
        {
            Query = query;
            Query.Projections.Clear();
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInit)
        {
            Visit(memberInit.NewExpression);

            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                if (binding.Expression.Is<QueryExpression>())
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

                Query.AddProjection(binding.Expression);
                Context.ProjectionOwner.AddProjection(binding.Expression, Query);
            }

            return memberInit;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                if (newExpression.Arguments[i].Is<QueryExpression>())
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

                Query.AddProjection(newExpression.Arguments[i]);
                Context.ProjectionOwner.AddProjection(newExpression.Arguments[i], Query);
            }

            return newExpression;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            var query = Context.QueryMapper.GetMappedQuery(parameter);
            if (query == null)
            {
                return parameter;
            }

            foreach (var item in query.GenQueryProjections())
            {
                Query.AddProjection(item);
                Context.ProjectionOwner.AddProjection(item, Query);
            }

            return parameter;
        }

        public override Expression Visit(Expression node)
        {
            node = base.Visit(node);

            if (node.NodeType == ExpressionType.New)
            {
                return VisitNew(node.As<NewExpression>());
            }

            if (node.NodeType == ExpressionType.MemberInit)
            {
                return VisitMemberInit(node.As<MemberInitExpression>());
            }

            return node;
        }
    }

    public class ModelRefrenceExpressionVisitor : ExpressionVisitorBase
    {
        public IQueryContext Context { get; }

        public QueryExpression Query { get; }

        public ParameterExpression QueryRefrence { get; }

        public ModelRefrenceExpressionVisitor(
            IQueryContext context,
            QueryExpression query,
            ParameterExpression queryRefrence)
        {
            Context = context;
            Query = query;
            QueryRefrence = queryRefrence;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            var modelElementType = parameter.Type.GetModelElementType();
            var query = parameter == QueryRefrence ? Query : Context.QueryMapper.GetMappedQuery(parameter);

            if (query != null)
            {
                var model = query.QueryModel?.GetModelExpression(modelElementType);
                if (model != null)
                {
                    return model;
                }

                return CreateModelInitExpression(modelElementType, parameter);
            }

            return parameter;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return node;
        }

        protected MemberInitExpression CreateModelInitExpression(Type modelEleType, Expression modelRefrence)
        {
            var modeEleTypeDescriptor = TypeDescriptorCacheFactory.Factory.Create(modelEleType);
            var constructorParameterMembers = modeEleTypeDescriptor
                .Constructor
                .GetParameters()
                .Select(item => item.Member)
                .ToList();

            var newExpression = CreateModelNewExpression(modeEleTypeDescriptor.Constructor, modelRefrence, constructorParameterMembers);
            var members = modeEleTypeDescriptor
                .MemberDescriptors
                .Where(item => !constructorParameterMembers.Contains(item.Member))
                .Select(item => item.Member)
                .ToList();

            if (members.Count == 0)
            {
                return Expression.MemberInit(newExpression);
            }

            var bindings = new List<MemberBinding>();

            foreach (var item in members)
            {
                bindings.Add(Expression.Bind(item, Expression.MakeMemberAccess(modelRefrence, item)));
            }

            return Expression.MemberInit(newExpression, bindings);
        }

        protected NewExpression CreateModelNewExpression(ConstructorInfo constructor, Expression modelRefrence, List<MemberInfo> parameterMembers)
        {
            if (parameterMembers.Count == 0)
            {
                return Expression.New(constructor);
            }

            var argumnets = new List<Expression>();

            foreach (var item in parameterMembers)
            {
                argumnets.Add(Expression.MakeMemberAccess(modelRefrence, item));
            }

            return Expression.New(constructor, argumnets);
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            var body = Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(body, lambda.Parameters);
            }

            return lambda;
        }
    }
}
