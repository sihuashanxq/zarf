using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query
{
    public class LinqExpressionTanslator : ILinqExpressionTanslator
    {
        public Expression Build(Expression node, QueryContext context)
        {
            var translatedExpression = new SqlTranslatingExpressionVisitor(context, NodeTypeTranslatorProvider.Default).Visit(node);

            if (translatedExpression.Is<QueryExpression>())
            {
                var rootQuery = translatedExpression.As<QueryExpression>();
                if (rootQuery.Projections.Count == 0)
                {
                    rootQuery.Projections.AddRange(rootQuery.GenerateColumns());
                }

                BuildResult(rootQuery, context);
                OptimizingColumns(rootQuery);
                return rootQuery;
            }
            else
            {
                context.ProjectionMappingProvider.Map(translatedExpression, translatedExpression, 0);
                return translatedExpression;
            }
        }

        public void BuildResult(QueryExpression rootQuery, QueryContext context)
        {
            if (rootQuery.Result != null)
            {
                var entityNewExpression = new ExpressionMemberMapVisitor(
                    rootQuery,
                    ToEntityNewExpression,
                    context
                 ).Visit(rootQuery.Result.EntityNewExpression);

                if (entityNewExpression != rootQuery.Result.EntityNewExpression)
                {
                    rootQuery.Result = new EntityResult(entityNewExpression, rootQuery.Result.ElementType);
                }

                return;
            }

            var entityNew = ToEntityNewExpression(
                    rootQuery,
                    rootQuery,
                    context
                );

            if (entityNew == null)
            {
                throw new Exception("QueryExpressionBuilder.BuildResult MemberInit Is NULL!");
            }

            rootQuery.Result = new EntityResult(entityNew, rootQuery.Type);
        }

        public void OptimizingColumns(QueryExpression query)
        {
            if (query.SubQuery == null)
            {
                return;
            }

            var subQueryProjections = new List<Expression>();

            foreach (var p in query.Projections)
            {
                var column = p.As<ColumnExpression>();
                if (column == null)
                {
                    var aggreate = p.As<AggregateExpression>();
                    if (aggreate != null && aggreate.KeySelector.Is<ColumnExpression>())
                    {
                        column = aggreate.KeySelector.As<ColumnExpression>();
                    }
                }

                foreach (var sColumn in query.SubQuery.Projections.OfType<ColumnExpression>())
                {
                    if (sColumn.Alias == column.Column.Name)
                    {
                        subQueryProjections.Add(sColumn);
                        break;
                    }
                }
            }

            foreach (var projection in query.SubQuery.Projections.Where(item => !item.Is<ColumnExpression>()))
            {
                subQueryProjections.Add(projection);
            }

            query.SubQuery.Projections.Clear();
            query.SubQuery.Projections.AddRange(subQueryProjections);

            OptimizingColumns(query.SubQuery);
        }

        /// <summary>
        /// 清醒后重构
        /// </summary>
        /// <param name="rootQuery"></param>
        /// <param name="targetQuery"></param>
        /// <param name="mappingProvider"></param>
        /// <returns></returns>
        public Expression ToEntityNewExpression(
            QueryExpression rootQuery,
            QueryExpression targetQuery,
            IQueryContext queryContext
        )
        {
            var modeType = targetQuery.Type;
            if (modeType.IsCollection())
            {
                modeType = modeType.GetCollectionElementType();
            }

            var modeTypeDescriptor = EntityTypeDescriptorFactory.Factory.Create(modeType);
            var bindings = new List<MemberBinding>();
            var modeNew = Expression.New(modeTypeDescriptor.Constructor);

            foreach (var member in modeTypeDescriptor.GetWriteableMembers())
            {
                var column = new ColumnExpression(targetQuery, member);
                var ordinal = QueryUtils.FindExpressionIndex(rootQuery, column);
                if (ordinal == -1)
                {
                    continue;
                }

                queryContext.ProjectionMappingProvider.Map(column, rootQuery, ordinal);
                bindings.Add(Expression.Bind(member, column));
            }

            foreach (var member in modeTypeDescriptor.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var navigation = queryContext.PropertyNavigationContext.GetNavigation(member);
                if (navigation == null)
                {
                    continue;
                }

                foreach (var item in navigation.RefrenceColumns)
                {
                    if (!item.Is<ColumnExpression>())
                    {
                        continue;
                    }

                    var column = item.As<ColumnExpression>();
                    var index = -1;
                    if (column.FromTable == rootQuery)
                    {
                        index = QueryUtils.FindExpressionIndex(rootQuery, item);
                        queryContext.ProjectionMappingProvider.Map(item, rootQuery, index);
                    }

                    if (index == -1)
                    {
                        throw new Exception("");
                    }
                }

                var propertyBinding = CreateIncludePropertyBinding(member, queryContext as QueryContext, queryContext.ProjectionMappingProvider);
                bindings.Add(propertyBinding);
            }

            var memInit = Expression.MemberInit(modeNew, bindings);

            if (!typeof(IEnumerable).IsAssignableFrom(targetQuery.Type))
            {
                return memInit;
            }
            else
            {
                //属性为集合 简单处理为List<T>
                var constructor = typeof(List<>).MakeGenericType(modeType).GetConstructor(Type.EmptyTypes);
                return Expression.ListInit(Expression.New(constructor), memInit);
            }
        }

        public MemberBinding CreateIncludePropertyBinding(MemberInfo memberInfo, QueryContext queryContext, IEntityProjectionMappingProvider mappingProvider)
        {
            var innerQuery = queryContext.PropertyNavigationContext.GetNavigation(memberInfo).RefrenceQuery;
            BuildResult(innerQuery, queryContext);

            var propertyElementType = memberInfo.GetMemberInfoType().GetCollectionElementType();
            var propertyEnumerableType = typeof(EntityEnumerable<>).MakeGenericType(propertyElementType);

            var newPropertyEnumearbles = Expression.Convert(
                    Expression.New(propertyEnumerableType.GetConstructor(new Type[] { typeof(Expression), typeof(EntityProjectionMappingProvider), typeof(QueryContext) }),
                    Expression.Constant(innerQuery), Expression.Constant(mappingProvider), Expression.Constant(queryContext)),
                    propertyEnumerableType
                );

            var contextInstance = Expression.Constant(queryContext);
            var propertyInstnance = Expression.Constant(memberInfo);
            var getEnumerables = Expression.Call(null, GetIncludeMemberInstanceMethodInfo, contextInstance, propertyInstnance);
            var setEnumerables = Expression.Call(null, SetIncludeMemberInstanceMethodInfo, contextInstance, propertyInstnance, newPropertyEnumearbles);

            var target = Expression.Label(propertyEnumerableType);
            var variable = Expression.Variable(propertyEnumerableType);

            var isNull = Expression.Equal(getEnumerables, Expression.Constant(null));

            var condtion = Expression.IfThen(isNull, setEnumerables);
            var setLocal = Expression.Assign(variable, Expression.Convert(getEnumerables, newPropertyEnumearbles.Type));

            var ret = Expression.Return(target, variable, propertyEnumerableType);
            var label = Expression.Label(target, variable);

            var block = Expression.Block(new[] { variable }, condtion, setLocal, ret, label);

            return Expression.Bind(memberInfo, block);
        }

        public static object GetIncludeMemberInstance(QueryContext queryContext, MemberInfo member)
        {
            if (queryContext.SubQueryInstance.TryGetValue(member, out object instance))
            {
                return instance;
            }

            return null;
        }

        public static void SetIncludeMemberInstance(QueryContext queryContext, MemberInfo member, object instance)
        {
            queryContext.SubQueryInstance[member] = instance;
        }


        public static MethodInfo GetIncludeMemberInstanceMethodInfo = typeof(LinqExpressionTanslator).GetMethod(nameof(GetIncludeMemberInstance));

        public static MethodInfo SetIncludeMemberInstanceMethodInfo = typeof(LinqExpressionTanslator).GetMethod(nameof(SetIncludeMemberInstance));

        public Expression Translate(Expression linqExpression)
        {
            throw new NotImplementedException();
        }

        public IQueryContext Context => throw new NotImplementedException();
    }
}
