using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Mapping.Bindings.Binders;
using Zarf.Query;
using Zarf.Query.Expressions;

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

        public Func<IDataReader, TEntity> Bind<TEntity>(IBindingContext context)
        {
            var bindQuery = context.Query.As<QueryExpression>()?.Result?.EntityNewExpression ?? context.Query;
            if (bindQuery.Is<LambdaExpression>())
            {
                bindQuery = bindQuery.As<LambdaExpression>().Body;
            }

            var query = context.Query;
            if (query.Is<AggregateExpression>())
            {
                query = query
                    .As<AggregateExpression>()
                    ?.KeySelector
                    ?.As<ColumnExpression>()
                    ?.FromTable;
                InitializeQueryColumns(query.As<QueryExpression>());
            }
            else if (query.Is<AllExpression>())
            {
                InitializeQueryColumns(query.As<AllExpression>().Expression.As<QueryExpression>());
            }
            else if (query.Is<AnyExpression>())
            {
                InitializeQueryColumns(query.As<AnyExpression>().Expression.As<QueryExpression>());
            }
            else
            {
                InitializeQueryColumns(query.As<QueryExpression>());
            }

            return (Func<IDataReader, TEntity>)Expression.Lambda(Visit(bindQuery), DataReader).Compile();
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
            if (node.Is<FromTableExpression>())
            {
                return BindQueryExpression(node.As<QueryExpression>());
            }

            var col = node;
            if (node.Is<AggregateExpression>())
            {
                col = node.As<AggregateExpression>().KeySelector;
            }
            else if (node.Is<AllExpression>() || node.Is<AnyExpression>())
            {
                return Expression.Call(
                    null,
                    MemberValueGetterProvider.Default.GetValueGetter(typeof(bool)),
                    DataReader,
                    Expression.Constant(0));
            }

            if (!ReflectionUtil.SimpleTypes.Contains(col?.Type))
            {
                if (!node.Type.IsValueType)
                {
                    return Expression.Constant(null);
                }
                else
                {
                    return Expression.Constant(Activator.CreateInstance(node.Type));
                }
            }

            var ordinal = ProjectionMappingProvider.GetOrdinal(col);
            var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(col.Type);
            if (ordinal == -1 || valueSetter == null)
            {
                throw new NotImplementedException($"列{col.As<ColumnExpression>()?.Column?.Name} 未包含在查询中!");
            }

            return Expression.Call(null, valueSetter, DataReader, Expression.Constant(ordinal));
        }

        protected override Expression VisitNew(NewExpression newExp)
        {
            if (newExp.Arguments.Count == 0)
            {
                return CreateEntityNewExpressionBlock(newExp.Constructor, newExp.Type);
            }

            var arguments = new List<Expression>();
            for (var i = 0; i < newExp.Arguments.Count; i++)
            {
                var argument = newExp.Arguments[i];
                var member = newExp.Members[i];

                if (Context.PropertyNavigationContext.IsPropertyNavigation(member))
                {
                    throw new Exception("Class Refrence A Navigation Property Must Have A Default Constructor!");
                }

                arguments.Add(Expression.Convert(Visit(newExp.Arguments[i]), argument.Type));
            }

            return CreateEntityNewExpressionBlock(newExp.Constructor, newExp.Type, arguments);
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
                        Expression = column ?? item.Expression,
                        Member = column?.Member,
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

            foreach (var item in typeDescriptor.GetExpandMembers())
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
                        memValueType.GetConstructor(new Type[] { typeof(Expression), typeof(IMemberValueCache), typeof(IDbContextParts) }),
                        Expression.Constant(memNavigation.RefrenceQuery),
                        Expression.Constant(Context.MemberValueCache),
                        Expression.Constant(Context.DbContextParts)),
                    memValueType
                );

            var getOrSetCachedMemberValue = Expression.Call(
                null,
                GetOrSetMemberValueMethod,
                Expression.Constant(Context.MemberValueCache),
                Expression.Constant(memberInfo),
                makeNewMemberValue);

            var blockBegin = Expression.Label(eObject.Type);
            var memValueVar = Expression.Variable(memValueType, "memValue");
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
        public static BlockExpression CreateEntityNewExpressionBlock(ConstructorInfo constructor, Type type, IEnumerable<Expression> arguments = null)
        {
            if (constructor == null)
            {
                throw new NotImplementedException($"Type:{type.FullName} need a conscrutor which is none of parameters!");
            }

            var begin = Expression.Label(type);
            var var = Expression.Variable(type);
            var varValue = Expression.Assign(var, Expression.New(constructor, arguments));
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
