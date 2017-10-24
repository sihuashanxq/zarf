using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Mapping.Bindings;
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
                if (rootQuery.ProjectionCollection.Count == 0)
                {
                    rootQuery.ProjectionCollection.AddRange(context.ProjectionScanner.Scan(rootQuery));
                }

                var enitty = new DefaultEntityBinder(context.ProjectionMappingProvider, context.PropertyNavigationContext, context, rootQuery);

                foreach (var item in rootQuery.ProjectionCollection)
                {
                    context.ProjectionMappingProvider.Map(item);
                }

                var y = Expression.Lambda(enitty.Bind(new BindingContext(rootQuery.Type, rootQuery.Result?.EntityNewExpression ?? null)
                {
                    MappingProvider = context.ProjectionMappingProvider,
                    CreationHandleProvider = new EntityCreationHandleProvider()

                }).As<LambdaExpression>().Body, DefaultEntityBinder.DataReader).Compile();
                context.func = y;

                return rootQuery;
            }
            else
            {
                context.ProjectionMappingProvider.Map(translatedExpression, translatedExpression, 0);
                return translatedExpression;
            }
        }

        /// <summary>
        /// 清醒后重构
        /// </summary>
        /// <param name="rootQuery"></param>
        /// <param name="fromTable"></param>
        /// <param name="mappingProvider"></param>
        /// <returns></returns>
        public Expression ToEntityNewExpression(
            QueryExpression rootQuery,
            FromTableExpression fromTable,
            IQueryContext context
        )
        {
            var entityType = fromTable.Type.GetCollectionElementType();
            var entityTypeDescriptor = EntityTypeDescriptorFactory.Factory.Create(entityType);
            var entityBindings = new List<MemberBinding>();
            var entityNew = Expression.New(entityTypeDescriptor.Constructor);

            foreach (var memberInfo in entityTypeDescriptor.GetWriteableMembers())
            {
                //TODO //使用这段代码更新 DefaultEntityBinder
                var column = new ColumnExpression(fromTable, memberInfo);
                if (context.EntityMemberMappingProvider.IsMapped(memberInfo))
                {
                    column = context.EntityMemberMappingProvider.GetExpression(memberInfo).As<ColumnExpression>();
                }

                var projection = QueryUtils.FindProjection(rootQuery, column);
                if (projection == null)
                {
                    continue;
                }

                context.ProjectionMappingProvider.Map(column, rootQuery, projection.Ordinal);
                context.ProjectionMappingProvider.Map(projection);
                entityBindings.Add(Expression.Bind(memberInfo, column));
            }

            foreach (var member in entityTypeDescriptor.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!context.PropertyNavigationContext.IsPropertyNavigation(member))
                {
                    continue;
                }

                var navigation = context.PropertyNavigationContext.GetNavigation(member);
                foreach (var item in navigation.RefrenceColumns)
                {
                    if (!item.Is<ColumnExpression>())
                    {
                        continue;
                    }
                }

                var binding = CreateIncludePropertyBinding(member, context as QueryContext);
                entityBindings.Add(binding);
            }

            var memInit = Expression.MemberInit(entityNew, entityBindings);
            if (!fromTable.Type.IsCollection())
            {
                return memInit;
            }
            else
            {
                //属性为集合 简单处理为List<T>
                var constructor = typeof(List<>).MakeGenericType(entityType).GetConstructor(Type.EmptyTypes);
                return Expression.ListInit(Expression.New(constructor), memInit);
            }
        }

        public Expression Translate(Expression linqExpression)
        {
            return Build(linqExpression, Context as QueryContext);
        }

        public IQueryContext Context { get; }

        public LinqExpressionTanslator(IQueryContext context)
        {
            Context = context;
        }
    }
}
