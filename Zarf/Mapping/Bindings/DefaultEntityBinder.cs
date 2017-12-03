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
        public static readonly ParameterExpression DataReader = Expression.Parameter(typeof(IDataReader), "reader");

        public IEntityProjectionMappingProvider ProjectionMappingProvider { get; }

        public IPropertyNavigationContext NavigationContext { get; }

        public IQueryContext Context { get; }

        protected QueryExpression RootQuery { get; set; }

        protected static long VariablesCounter = 0;

        const string VarPrefix = "_var_";

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
                RootQuery = query.As<AggregateExpression>()
                    ?.KeySelector?.As<ColumnExpression>()
                    ?.Query.As<QueryExpression>();
            }
            else if (query.Is<AllExpression>())
            {
                RootQuery = query.As<AllExpression>().Expression.As<QueryExpression>();
            }
            else if (query.Is<AnyExpression>())
            {
                RootQuery = query.As<AnyExpression>().Expression.As<QueryExpression>();
            }
            else
            {
                RootQuery = query.As<QueryExpression>();
            }

            InitializeQueryColumns(RootQuery);

            var body = Visit(bindQuery);
            return (Func<IDataReader, TEntity>)Expression.Lambda(body, DataReader).Compile();
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
            if (node.Is<QueryExpression>())
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
                if (Context.PropertyNavigationContext.IsPropertyNavigation(newExp.Members[i]))
                {
                    throw new Exception("Class Refrence A Navigation Property Must Have A Default Constructor!");
                }
                var argument = Visit(newExp.Arguments[i]);
                var member = newExp.Members[i];
                arguments.Add(argument);
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

        protected void InitializeQueryColumns(QueryExpression query)
        {
            if (query == null)
            {
                return;
            }

            if (query.Projections.Count == 0)
            {
                foreach (var item in query.GenerateTableColumns())
                {
                    var projection = new ColumnDescriptor()
                    {
                        Member = item.Member,
                        Expression = item,
                        Ordinal = query.Projections.Count
                    };

                    query.Projections.Add(projection);
                    Context.ProjectionMappingProvider.Map(projection);
                }
                return;
            }

            foreach (var item in query.Projections)
            {
                var col = item;
                if (item.Expression.Is<AggregateExpression>())
                {
                    var column = item.Expression.As<AggregateExpression>().KeySelector.As<ColumnExpression>();
                    col = new ColumnDescriptor()
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

        protected Expression BindQueryExpression(QueryExpression query)
        {
            if (query == null)
            {
                return null;
            }

            var eType = query.Type.GetCollectionElementType();
            var typeDescriptor = TypeDescriptorCacheFactory.Factory.Create(eType);

            if (typeDescriptor.Constructor.GetParameters().Length == 0)
            {
                var memberExpressions = new List<MemberExpressionPair>();
                var eNewBlock = CreateEntityNewExpressionBlock(typeDescriptor.Constructor, typeDescriptor.Type);

                foreach (var item in typeDescriptor.MemberDescriptors)
                {
                    var mappedExpression = FindMemberRelatedExpression(query, item.Member);
                    if (mappedExpression != null)
                    {
                        memberExpressions.Add(new MemberExpressionPair(item.Member, mappedExpression));
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
            else
            {
                var activatingArgs = new List<Expression>();
                foreach (var item in typeDescriptor.MemberDescriptors)
                {
                    activatingArgs.Add(Visit(FindMemberRelatedExpression(query, item.Member)));
                }

                return CreateEntityNewExpressionBlock(typeDescriptor.Constructor, typeDescriptor.Type, activatingArgs);
            }
        }

        public BlockExpression CreateIncludePropertyBinding(MemberInfo memberInfo, ParameterExpression eObject)
        {
            var memNavigation = NavigationContext.GetNavigation(memberInfo);
            var memEleType = memberInfo.GetPropertyType().GetCollectionElementType();
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
                ReflectionUtil.SubQueryWhere.MakeGenericMethod(eObject.Type, memEleType),
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
        public static BlockExpression CreateEntityNewExpressionBlock(
            ConstructorInfo constructor,
            Type type,
            IEnumerable<Expression> arguemnts = null)
        {
            if (constructor == null)
            {
                throw new NotImplementedException($"Type:{type.FullName} need a conscrutor which is none of parameters!");
            }

            var ctorArgs = new List<Expression>();
            var propertyValues = new List<Expression>();
            var propertyDeclares = new List<ParameterExpression>();

            foreach (var argument in arguemnts ?? new List<Expression>())
            {
                if (argument.NodeType != ExpressionType.Block)
                {
                    ctorArgs.Add(argument);
                    continue;
                }

                var block = argument.As<BlockExpression>();
                ctorArgs.Add(block.Variables[0]);
                propertyDeclares.Add(block.Variables[0]);
                propertyValues.AddRange(block.Expressions.Take(block.Expressions.Count - 2));
            }

            var beginOfBlock = Expression.Label(type, VarPrefix + VariablesCounter++);
            var varOfEntity = Expression.Variable(type, VarPrefix + VariablesCounter++);
            var valueOfEntity = Expression.New(constructor, arguemnts == null ? null : ctorArgs);
            var setVarOfEntityValue = Expression.Assign(varOfEntity, valueOfEntity);
            var returnVarOfEntity = Expression.Return(beginOfBlock, varOfEntity);
            var endOfBlock = Expression.Label(beginOfBlock, varOfEntity);

            propertyDeclares.Add(varOfEntity);
            propertyValues.Add(setVarOfEntityValue);
            propertyValues.Add(returnVarOfEntity);
            propertyValues.Add(endOfBlock);

            return Expression.Block(propertyDeclares, propertyValues.ToArray());
        }

        public Expression FindMemberRelatedExpression(QueryExpression container, MemberInfo member)
        {
            foreach (var item in RootQuery.Projections)
            {
                var col = item.Expression.As<ColumnExpression>();
                var aggrate = item.Expression.As<AggregateExpression>();
                if (aggrate != null)
                {
                    col = aggrate.KeySelector.As<ColumnExpression>();
                }

                if (col != null && col.Member == member && col.Query == container)
                {
                    return col;
                }
            }

            return null;
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
