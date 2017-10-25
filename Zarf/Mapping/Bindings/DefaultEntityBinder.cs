using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Mapping.Bindings.Binders;
using Zarf.Query;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Mapping.Bindings
{
    /// <summary>
    /// 默认实体绑定实现
    /// </summary>
    public class DefaultEntityBinder : ExpressionVisitor, IBinder
    {
        public static readonly ParameterExpression DataReader = Expression.Parameter(typeof(IDataReader));

        public IEntityProjectionMappingProvider ProjectionMappingProvider { get; }

        public IPropertyNavigationContext NavigationContext { get; }

        public Expression Query { get; }

        public IQueryContext Context { get; }

        public DefaultEntityBinder(
            IEntityProjectionMappingProvider projectionMappingProvider,
            IPropertyNavigationContext navigationContext,
            IQueryContext context,
            Expression query)
        {
            Query = query;
            NavigationContext = navigationContext;
            ProjectionMappingProvider = projectionMappingProvider;
            Context = context;
        }

        public Expression Bind(IBindingContext context)
        {
            if (context.BindExpression.Is<LambdaExpression>())
            {
                return Visit(context.BindExpression.As<LambdaExpression>().Body);
            }
            return Visit(context.BindExpression);
        }

        protected override Expression VisitMemberInit(MemberInitExpression memInit)
        {
            var eNewBlock = Visit(memInit.NewExpression) as BlockExpression;
            return BindMembers(eNewBlock,
                memInit.Bindings.OfType<MemberAssignment>().Select(item => item.Member).ToList(),
                memInit.Bindings.OfType<MemberAssignment>().Select(item => item.Expression).ToList());
        }

        protected override Expression VisitExtension(Expression exp)
        {
            ColumnExpression column = null;
            if (exp.Is<ColumnExpression>())
            {
                column = exp.As<ColumnExpression>();
            }

            if (exp.Is<AggregateExpression>())
            {
                column = exp.As<AggregateExpression>().KeySelector.As<ColumnExpression>();
            }

            if (column != null)
            {
                var ordinal = ProjectionMappingProvider.GetOrdinal(Query, column);
                var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(column.Type);
                if (ordinal == -1 || valueSetter == null)
                {
                    throw new NotImplementedException($"列{column.Column.Name} 未包含在查询中!");
                }

                return Expression.Call(null, valueSetter, DataReader, Expression.Constant(ordinal));
            }

            if (exp.Is<FromTableExpression>())
            {
                return BindQueryExpression(exp.As<QueryExpression>());
            }
            else
            {
                throw new NotImplementedException($"不支持{exp.GetType().Name} 到 {exp.Type.Name}的转换!!!");
            }
        }

        protected override Expression VisitNew(NewExpression newExp)
        {
            var constructorInfo = newExp.Constructor;
            if (newExp.Constructor.GetParameters().Length != 0)
            {
                constructorInfo = newExp.Type.GetConstructor(Type.EmptyTypes);
            }

            var eNewBlock = CreateEntityNewExpressionBlock(constructorInfo, newExp.Type);
            if (newExp.Arguments.Count == 0)
            {
                return eNewBlock;
            }

            return BindMembers(eNewBlock, newExp.Members.ToList(), newExp.Arguments.ToList());
        }

        protected BlockExpression BindMembers(BlockExpression eNewBlock, List<MemberInfo> mems, List<Expression> expes)
        {
            var memBindings = new List<Expression>();
            var entity = eNewBlock.Variables.FirstOrDefault();
            var vars = new List<ParameterExpression>(eNewBlock.Variables);

            for (var i = 0; i < mems.Count; i++)
            {
                if (NavigationContext.IsPropertyNavigation(mems[i]))
                {
                    var block = CreateIncludePropertyBinding(mems[i], eNewBlock.Variables.FirstOrDefault());
                    vars.AddRange(block.Variables);
                    memBindings.AddRange(block.Expressions);
                }
                else
                {
                    var argument = Visit(expes[i]);
                    var memAccess = Expression.MakeMemberAccess(eNewBlock.Variables.FirstOrDefault(), mems[i]);
                    memBindings.Add(Expression.Assign(memAccess, argument));
                }
            }

            var nodes = eNewBlock.Expressions.ToList();
            var retIndex = nodes.FindLastIndex(item => item is GotoExpression);
            if (retIndex == -1)
            {
                throw new Exception();
            }

            nodes.InsertRange(retIndex, memBindings);
            return eNewBlock.Update(vars, nodes);
        }

        protected Expression BindQueryExpression(QueryExpression query)
        {
            if (query == null)
            {
                return null;
            }

            var typeDescriptor = EntityTypeDescriptorFactory.Factory.Create(query.Type);
            var memExpressions = new List<Expression>();
            var members = new List<MemberInfo>();
            var eNewBlock = CreateEntityNewExpressionBlock(typeDescriptor.Constructor, typeDescriptor.Type);

            foreach (var item in typeDescriptor.GetWriteableMembers())
            {
                var bindExpression = FindMemberRelatedExpression(query, item);
                if (bindExpression != null)
                {
                    memExpressions.Add(bindExpression);
                    members.Add(item);
                }
            }

            foreach (var item in typeDescriptor.Type.GetProperties().Where(item => item.SetMethod != null))
            {
                if (!members.Contains(item) && NavigationContext.IsPropertyNavigation(item))
                {
                    members.Add(item);
                    memExpressions.Add(null);
                }
            }

            return BindMembers(eNewBlock, members, memExpressions);
        }

        public BlockExpression CreateIncludePropertyBinding(MemberInfo memberInfo, ParameterExpression ownner)
        {
            var navigation = NavigationContext.GetNavigation(memberInfo);
            var innerQuery = navigation.RefrenceQuery;
            var propertyEleType = memberInfo.GetMemberTypeInfo().GetCollectionElementType();
            var propertyType = typeof(SubEntityEnumerable<>).MakeGenericType(propertyEleType);

            ////TODO :1
            var i = 0;
            foreach (var m in innerQuery.GenerateColumns())
            {
                ProjectionMappingProvider.Map(new Projection()
                {
                    Query = innerQuery,
                    Expression = m,
                    Ordinal = i++,
                });
            }

            var makeNewPropertyValue = Expression.Convert(
                    Expression.New(propertyType.GetConstructor(new Type[] { typeof(Expression), typeof(QueryContext) }),
                    Expression.Constant(innerQuery), Expression.Constant(Context)),
                    propertyType
                );

            var contextInstance = Expression.Constant(Context);
            var getStoredPropertyValue = Expression.Call(null, GetIncludeMemberInstanceMethodInfo, contextInstance, Expression.Constant(memberInfo));
            var toStorePropertyValue = Expression.Call(null, SetIncludeMemberInstanceMethodInfo, contextInstance, Expression.Constant(memberInfo), makeNewPropertyValue);

            var blockBegin = Expression.Label(ownner.Type);
            var propertyValueVar = Expression.Variable(propertyType);

            var isStoredPropertyValueNull = Expression.Equal(getStoredPropertyValue, Expression.Constant(null));
            var toStorePropertyValueIfNull = Expression.IfThen(isStoredPropertyValueNull, toStorePropertyValue);
            var setPropertyValueVar = Expression.Assign(propertyValueVar, Expression.Convert(getStoredPropertyValue, makeNewPropertyValue.Type));

            var condtion = navigation.Relation.UnWrap().As<LambdaExpression>();
            var updatedCondtion = new InnerNodeUpdateExpressionVisitor(condtion.Parameters.First(), ownner)
                 .Update(condtion)
                 .As<LambdaExpression>();

            var propertyValue = Expression.Call(null, ReflectionUtil.EnumerableWhereMethod.MakeGenericMethod(ownner.Type, propertyEleType), propertyValueVar, ownner, updatedCondtion);
            var setPropertyValue = Expression.Call(ownner, memberInfo.As<PropertyInfo>().SetMethod, propertyValue);
            var blockEnd = Expression.Label(blockBegin, ownner);

            return Expression.Block(
                new[] { propertyValueVar },
                toStorePropertyValueIfNull,
                setPropertyValueVar,
                setPropertyValue,
                blockEnd);
        }

        public static object GetIncludeMemberInstance(IQueryContext queryContext, MemberInfo member)
        {
            if (queryContext.SubQueryInstance.TryGetValue(member, out object instance))
            {
                return instance;
            }

            return null;
        }

        public static void SetIncludeMemberInstance(IQueryContext queryContext, MemberInfo member, object instance)
        {
            queryContext.SubQueryInstance[member] = instance;
        }

        public static MethodInfo GetIncludeMemberInstanceMethodInfo = typeof(DefaultEntityBinder).GetMethod(nameof(GetIncludeMemberInstance));

        public static MethodInfo SetIncludeMemberInstanceMethodInfo = typeof(DefaultEntityBinder).GetMethod(nameof(SetIncludeMemberInstance));

        /// <summary>
        /// {
        ///     var entity=new Entity();
        ///     return entity;
        /// }
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static BlockExpression CreateEntityNewExpressionBlock(ConstructorInfo constructor, Type type)
        {
            if (constructor == null)
            {
                throw new NotImplementedException($"Type:{type.FullName} need a conscrutor which is none of parameters!");
            }

            var begin = Expression.Label(type);

            var var = Expression.Variable(type);
            var varValue = Expression.Assign(var, Expression.New(constructor));
            var retVar = Expression.Return(begin, var);

            var end = Expression.Label(begin, var);
            return Expression.Block(new[] { var }, varValue, retVar, end);
        }

        public static Expression FindMemberRelatedExpression(QueryExpression query, MemberInfo member)
        {
            if (query.ProjectionCollection?.Count == 0 &&
                query.SubQuery != null)
            {
                return FindMemberRelatedExpression(query.SubQuery, member);
            }

            return query.ProjectionCollection.FirstOrDefault(item => item.Member == member)?.Expression;
        }
    }
}
