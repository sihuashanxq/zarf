using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class MemberExpressionPair
    {
        public MemberInfo Member { get; set; }

        public Expression Expression { get; set; }

        public MemberExpressionPair(MemberInfo mem, Expression exp)
        {
            Member = mem;
            Expression = exp;
        }
    }

    /// <summary>
    /// 默认实体绑定实现
    /// </summary>
    public class DefaultEntityBinder : ExpressionVisitor, IBinder
    {
        public static readonly ParameterExpression DataReader = Expression.Parameter(typeof(IDataReader));

        public IEntityProjectionMappingProvider ProjectionMappingProvider { get; }

        public IPropertyNavigationContext NavigationContext { get; }

        public IQueryContext Context { get; }

        public DefaultEntityBinder(IQueryContext context)
        {
            NavigationContext = context.PropertyNavigationContext;
            ProjectionMappingProvider = context.ProjectionMappingProvider;
            Context = context;
        }

        public Expression Bind(IBindingContext context)
        {
            var exp = context.Expression;
            if (exp.Is<LambdaExpression>())
            {
                exp = context.Expression.As<LambdaExpression>().Body;
            }

            if (exp.Is<AggregateExpression>())
            {
                var query = exp
                    .As<AggregateExpression>()
                    ?.KeySelector
                    ?.As<ColumnExpression>()
                    ?.FromTable.As<QueryExpression>();
                InitializeQueryColumns(query);
            }
            else
            {
                InitializeQueryColumns(exp.As<QueryExpression>());
            }

            return Visit(exp);
        }

        protected override Expression VisitMemberInit(MemberInitExpression memInit)
        {
            var eNewBlock = Visit(memInit.NewExpression) as BlockExpression;
            var memberExpressions = new List<MemberExpressionPair>();
            foreach (var memBinding in memInit.Bindings.OfType<MemberAssignment>())
            {
                memberExpressions.Add(new MemberExpressionPair(memBinding.Member, memBinding.Expression));
            }

            return BindMembers(eNewBlock, memberExpressions);
        }

        protected override Expression VisitExtension(Expression node)
        {
            ColumnExpression col = null;
            if (node.Is<ColumnExpression>())
            {
                col = node.As<ColumnExpression>();
            }

            if (node.Is<AggregateExpression>())
            {
                col = node.As<AggregateExpression>().KeySelector.As<ColumnExpression>();
            }

            if (col != null)
            {
                var ordinal = ProjectionMappingProvider.GetOrdinal(col);
                var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(col.Type);
                if (ordinal == -1 || valueSetter == null)
                {
                    throw new NotImplementedException($"列{col.Column.Name} 未包含在查询中!");
                }

                return Expression.Call(null, valueSetter, DataReader, Expression.Constant(ordinal));
            }

            if (node.Is<FromTableExpression>())
            {
                return BindQueryExpression(node.As<QueryExpression>());
            }
            else
            {
                throw new NotImplementedException($"不支持{node.GetType().Name} 到 {node.Type.Name}的转换!!!");
            }
        }

        protected override Expression VisitNew(NewExpression newExp)
        {
            var eNewBlock = CreateEntityNewExpressionBlock(newExp.Constructor, newExp.Type);
            if (newExp.Arguments.Count == 0)
            {
                return eNewBlock;
            }

            var memberExpressions = new List<MemberExpressionPair>();
            for (var i = 0; i < newExp.Arguments.Count; i++)
            {
                memberExpressions.Add(new MemberExpressionPair(newExp.Members[i], newExp.Arguments[i]));
            }

            return BindMembers(eNewBlock, memberExpressions);
        }

        protected BlockExpression BindMembers(BlockExpression eNewBlock, List<MemberExpressionPair> memberExpressions)
        {
            var eObject = eNewBlock.Variables.FirstOrDefault();
            var blockVars = new List<ParameterExpression>(eNewBlock.Variables);
            var memBindings = new List<Expression>();

            foreach (var memberExpression in memberExpressions)
            {
                if (NavigationContext.IsPropertyNavigation(memberExpression.Member))
                {
                    var memValuesBlock = CreateIncludePropertyBinding(memberExpression.Member, eObject);
                    blockVars.AddRange(memValuesBlock.Variables);
                    memBindings.AddRange(memValuesBlock.Expressions);
                }
                else
                {
                    var memberValue = Visit(memberExpression.Expression);
                    var memberAccess = Expression.MakeMemberAccess(eObject, memberExpression.Member);
                    memBindings.Add(Expression.Assign(memberAccess, memberValue));
                }
            }

            var memValues = eNewBlock.Expressions.ToList();
            var retIndex = memValues.FindLastIndex(item => item is GotoExpression);
            if (retIndex != -1)
            {
                memValues.InsertRange(retIndex, memBindings);
                return eNewBlock.Update(blockVars, memValues);
            }

            return eNewBlock;
        }

        protected void InitializeQueryColumns(QueryExpression qExpression)
        {
            if (qExpression == null)
            {
                return;
            }

            if (qExpression.Projections.Count == 0)
            {
                foreach (var item in qExpression.GenerateColumns())
                {
                    var projection = new Projection()
                    {
                        Member = item.Member,
                        Expression = item,
                        Ordinal = qExpression.Projections.Count
                    };

                    qExpression.Projections.Add(projection);
                    Context.ProjectionMappingProvider.Map(projection);
                }
                return;
            }

            foreach (var item in qExpression.Projections)
            {
                var col = item;
                if (item.Expression.Is<AggregateExpression>())
                {
                    var column = item.Expression.As<AggregateExpression>().KeySelector.As<ColumnExpression>();
                    col = new Projection()
                    {
                        Expression = column,
                        Member = column.Member,
                        Ordinal = item.Ordinal
                    };
                }

                if (!Context.ProjectionMappingProvider.IsMapped(col.Expression))
                {
                    Context.ProjectionMappingProvider.Map(col);
                }
            }
        }

        protected Expression BindQueryExpression(QueryExpression qExpression)
        {
            if (qExpression == null)
            {
                return null;
            }

            var eType = qExpression.Type.GetCollectionElementType();
            var typeDescriptor = EntityTypeDescriptorFactory.Factory.Create(eType);
            var memberExpressions = new List<MemberExpressionPair>();
            var eNewBlock = CreateEntityNewExpressionBlock(typeDescriptor.Constructor, typeDescriptor.Type);

            foreach (var item in typeDescriptor.GetWriteableMembers())
            {
                var mappedExpression = FindMemberRelatedExpression(qExpression, item);
                if (mappedExpression != null)
                {
                    memberExpressions.Add(new MemberExpressionPair(item, mappedExpression));
                }
            }

            foreach (var item in typeDescriptor.Type.GetProperties().Where(item => item.SetMethod != null))
            {
                if (NavigationContext.IsPropertyNavigation(item))
                {
                    memberExpressions.Add(new MemberExpressionPair(item, null));
                }
            }

            return BindMembers(eNewBlock, memberExpressions);
        }

        public BlockExpression CreateIncludePropertyBinding(MemberInfo memberInfo, ParameterExpression eObject)
        {
            var memNavigation = NavigationContext.GetNavigation(memberInfo);
            var memEleType = memberInfo.GetMemberTypeInfo().GetCollectionElementType();
            var memValueType = typeof(EntityPropertyEnumerable<>).MakeGenericType(memEleType);

            var makeNewMemberValue = Expression.Convert(
                    Expression.New(
                        memValueType.GetConstructor(new Type[] { typeof(Expression), typeof(IMemberValueCache) }),
                        Expression.Constant(memNavigation.RefrenceQuery),
                        Expression.Constant(Context.MemberValueCache)),
                    memValueType
                );

            var getOrSetCachedMemberValue = Expression.Call(
                null,
                GetOrSetMemberValueMethod,
                Expression.Constant(Context.MemberValueCache),
                Expression.Constant(memberInfo),
                makeNewMemberValue);

            var blockBegin = Expression.Label(eObject.Type);
            var memValueVar = Expression.Variable(memValueType);
            var assignMemberVar = Expression.Assign(memValueVar, Expression.Convert(getOrSetCachedMemberValue, makeNewMemberValue.Type));

            var filteredMemberValue = Expression.Call(
                null,
                ReflectionUtil.EnumerableWhereMethod.MakeGenericMethod(eObject.Type, memEleType),
                memValueVar,
                eObject,
                memNavigation.Relation.UnWrap().As<LambdaExpression>());

            var assignMemberValue = Expression.Call(
                eObject,
                memberInfo.As<PropertyInfo>().SetMethod,
                filteredMemberValue);

            var blockEnd = Expression.Label(blockBegin, eObject);

            return Expression.Block(
                new[] { memValueVar },
                assignMemberVar,
                assignMemberValue,
                blockEnd);
        }

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
            if (query.Projections?.Count == 0 &&
                query.SubQuery != null)
            {
                return FindMemberRelatedExpression(query.SubQuery, member);
            }

            return query.Projections.FirstOrDefault(item => item.Member == member)?.Expression;
        }

        public static object GetOrSetMemberValue(IMemberValueCache valueCache, MemberInfo member, object value)
        {
            var v = valueCache.GetValue(member);
            if (v == null)
            {
                valueCache.SetValue(member, value);
                return value;
            }

            return v;
        }

        public static MethodInfo GetOrSetMemberValueMethod = typeof(DefaultEntityBinder).GetMethod(nameof(GetOrSetMemberValue));
    }
}
