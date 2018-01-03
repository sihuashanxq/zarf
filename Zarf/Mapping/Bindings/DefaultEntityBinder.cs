using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Core;
using Zarf.Entities;
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

        public IQueryContext Context { get; }

        public ExpressionEqualityComparer ExpressionEquality { get; }

        protected QueryExpression Query { get; set; }

        protected static long VariablesCounter = 0;

        const string VarPrefix = "_var_";

        public DefaultEntityBinder(IQueryContext context)
        {
            Context = context;
            ExpressionEquality = new ExpressionEqualityComparer();
        }

        public Delegate Bind<TEntity>(IBindingContext context)
        {
            var model = context.Query.As<QueryExpression>()?.QueryModel?.Model ?? context.Query;

            var query = context.Query;
            if (query.Is<AggregateExpression>())
            {
                Query = query.As<AggregateExpression>()
                    ?.KeySelector?.As<ColumnExpression>()
                    ?.Query.As<QueryExpression>();
            }
            else if (query.Is<AllExpression>())
            {
                Query = query.As<AllExpression>().Query.As<QueryExpression>();
            }
            else if (query.Is<AnyExpression>())
            {
                Query = query.As<AnyExpression>().Query.As<QueryExpression>();
            }
            else
            {
                Query = query.As<QueryExpression>();
            }

            var modelCreation = Visit(model);
            return Expression.Lambda(modelCreation, DataReader).Compile();
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            for (var x = 0; x < Query.Projections.Count; x++)
            {
                var y = Query.Projections[x];

                if (y.Is<AggregateExpression>())
                {
                    y = y.As<AggregateExpression>().KeySelector;
                }

                if (y.Is<AliasExpression>())
                {
                    y = y.As<AliasExpression>().Expression;
                }

                if (new ExpressionEqualityComparer().Equals(y, constant))
                {
                    var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(constant.Type);
                    return Expression.Call(null, valueSetter, DataReader, Expression.Constant(x));
                }
            }

            return constant;
        }

        public override Expression Visit(Expression expression)
        {
            var queryModel = Context.QueryModelMapper.GetQueryModel(expression);
            if (queryModel != null)
            {
                return CreateSubQueryModel(queryModel);
            }

            var mappedProjection = Query.ExpressionMapper.GetMappedProjection(expression);
            if (mappedProjection != null)
            {
                var n = Query.ExpressionMapper.GetMappedProjection(mappedProjection);
                if (n != null)
                {
                    mappedProjection = n;
                }
            }

            if (mappedProjection == null)
            {
                return base.Visit(expression);
            }

            for (var x = 0; x < Query.Projections.Count; x++)
            {
                var y = Query.Projections[x];

                if (new ExpressionEqualityComparer().Equals(y, mappedProjection))
                {
                    var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(mappedProjection.Type);
                    return Expression.Call(null, valueSetter, DataReader, Expression.Constant(x));
                }
            }

            return base.Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            var argument = Context.MemberBindingMapper.GetMapedExpression(member);
            if (argument == null)
            {
                var queryModel = Context.QueryModelMapper.GetQueryModel(member.Expression);
                if (queryModel == null)
                {
                    queryModel = Context.QueryModelMapper.GetQueryModel(member);
                    var y = Visit(member.Expression);
                }

                var modelExpression = queryModel.GetModelExpression(member.Member.DeclaringType);
                if (modelExpression != null)
                {
                    argument = Context.MemberBindingMapper.GetMapedExpression(Expression.MakeMemberAccess(modelExpression, member.Member));
                }
            }

            for (var x = 0; x < Query.Projections.Count; x++)
            {
                var mapped = Query.ExpressionMapper.GetMappedProjection(argument);
                if (mapped != null)
                {
                    argument = mapped;
                }

                var y = Query.Projections[x];

                if (new ExpressionEqualityComparer().Equals(y, argument))
                {
                    var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(argument.Type);
                    argument = Expression.Call(null, valueSetter, DataReader, Expression.Constant(x));
                    break;
                }
            }

            return argument;
        }

        protected override Expression VisitMemberInit(MemberInitExpression memInit)
        {
            var eNewBlock = Visit(memInit.NewExpression) as BlockExpression;
            var memberExpressions = new List<MemberExpressionPair>();
            foreach (var memBinding in memInit.Bindings.OfType<MemberAssignment>())
            {
                var par = memBinding.Expression.As<MemberExpression>().Expression;
                var model = Context.QueryModelMapper.GetQueryModel(par);
                var argument = Context.MemberBindingMapper.GetMapedExpression(Expression.MakeMemberAccess(model.Model, memBinding.Member));
                for (var x = 0; x < Query.Projections.Count; x++)
                {
                    var mapped = Query.ExpressionMapper.GetMappedProjection(argument);
                    if (mapped != null)
                    {
                        argument = mapped;
                    }

                    if (new ExpressionEqualityComparer().Equals(Query.Projections[x], argument))
                    {
                        var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(argument.Type);
                        argument = Expression.Call(null, valueSetter, DataReader, Expression.Constant(x));
                        break;
                    }
                }

                memberExpressions.Add(new MemberExpressionPair(memBinding.Member, argument));
            }

            return BindMembers(eNewBlock, memberExpressions);
        }

        protected virtual Expression BindQueryProjection(Expression projection)
        {
            if (projection == null) return null;

            for (var i = 0; i < Query.Projections.Count; i++)
            {
                var mappedProjection = Query.ExpressionMapper.GetMappedProjection(projection);
                if (mappedProjection != null)
                {
                    projection = mappedProjection;
                }

                if (ExpressionEquality.Equals(Query.Projections[i], projection))
                {
                    var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(projection.Type);
                    return Expression.Call(null, valueSetter, DataReader, Expression.Constant(i));
                }
            }

            return null;
        }

        protected virtual Expression GetMemberBindingExpression(QueryEntityModel queryModel, MemberInfo member)
        {
            while (queryModel != null)
            {
                if (member.DeclaringType != queryModel.ModelElementType) continue;

                var memberExpression = Expression.MakeMemberAccess(queryModel.Model, member);
                var projection = Context.MemberBindingMapper.GetMapedExpression(memberExpression);

                if (projection == null)
                {
                    queryModel = queryModel?.Previous;
                    continue;
                }

                return projection;
            }

            return null;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            var queryModel = Query.QueryModel.FindQueryModel(newExpression);
            if (queryModel == null && newExpression.Arguments.Count != 0)
            {
                throw new Exception();
            }

            var constructorArguments = new List<Expression>();

            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                var bindingExpression = GetMemberBindingExpression(queryModel, newExpression.Members[i]);
                if (!newExpression.Arguments[i].Type.IsPrimtiveType())
                {
                    //子查询 ToList First
                    if (bindingExpression is QueryExpression query)
                    {
                        bindingExpression = CreateSubQueryModel(query.QueryModel);
                    }
                    else
                    {
                        bindingExpression = Visit(newExpression.Arguments[i]);
                    }

                    constructorArguments.Add(bindingExpression);
                    continue;
                }

                //此处是简单类型,应该是聚合
                if (bindingExpression is QueryExpression)
                {
                    bindingExpression = Visit(newExpression.Arguments[i]);
                }
                else
                {
                    bindingExpression = BindQueryProjection(bindingExpression);
                }

                if (bindingExpression == null)
                {
                    throw new Exception("projection not found!");
                }

                constructorArguments.Add(bindingExpression);
            }

            return CreateEntityNewExpressionBlock(newExpression.Constructor, newExpression.Type, constructorArguments);
        }

        protected BlockExpression BindMembers(BlockExpression eNewBlock, List<MemberExpressionPair> memberExpressions)
        {
            var eObject = eNewBlock.Variables.FirstOrDefault();
            var blockVars = new List<ParameterExpression>(eNewBlock.Variables);
            var memBindings = new List<Expression>();

            foreach (var memberExpression in memberExpressions)
            {
                var memberAccess = Expression.MakeMemberAccess(eObject, memberExpression.Member);
                memBindings.Add(Expression.Assign(memberAccess, memberExpression.Expression));
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

        protected virtual Expression CreateSubQueryModel(QueryEntityModel queryModel)
        {
            var modelType = typeof(EntityPropertyEnumerable<>).MakeGenericType(queryModel.ModelElementType);
            var modelEleNew = Expression.New(
                        modelType.GetConstructor(new Type[] { typeof(Expression), typeof(IQueryContext), typeof(IDbContextParts) }),
                        Expression.Constant(queryModel.Query),
                        Expression.Constant(Context),
                        Expression.Constant(Context.DbContextParts));


            var model = Expression.Convert(Expression.Call(
                    null,
                    GetOrSetMemberValueMethod,
                    Expression.Constant(Context.MemberValueCache),
                    Expression.Constant(queryModel),
                    modelEleNew),
                    typeof(IEnumerable<>).MakeGenericType(queryModel.ModelElementType)
                );

            if (queryModel.ModelElementType == queryModel.ModeType)
            {
                return Expression.Call(
                    null,
                    QueryEnumerable.FirstOrDefaultMethod.MakeGenericMethod(queryModel.ModelElementType),
                    model);
            }

            return Expression.Call(
                null,
                QueryEnumerable.ToListMethod.MakeGenericMethod(queryModel.ModelElementType),
                model);
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

        public static object GetOrAddSubQueryValue(ISubQueryValueCache valueCache, QueryEntityModel queryModel, object value)
        {
            var v = valueCache.GetValue(queryModel);
            if (v == null)
            {
                valueCache.SetValue(queryModel, value);
                return value;
            }

            return v;
        }

        public static MethodInfo GetOrSetMemberValueMethod = typeof(DefaultEntityBinder).GetMethod(nameof(GetOrAddSubQueryValue));
    }
}
