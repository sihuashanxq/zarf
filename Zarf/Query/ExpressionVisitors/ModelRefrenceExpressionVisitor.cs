using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Queries.Expressions;

namespace Zarf.Queries.ExpressionVisitors
{
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
    }
}
