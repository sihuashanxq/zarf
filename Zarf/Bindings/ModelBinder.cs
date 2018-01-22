﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query;
using Zarf.Query.Expressions;

namespace Zarf.Bindings
{
    /// <summary>
    /// 默认实体绑定实现
    /// </summary>
    public class ModelBinder : ExpressionVisitor, IModelBinder
    {
        public static readonly ParameterExpression DataReader = Expression.Parameter(typeof(IDataReader), "reader");

        public IQueryContext QueryContext { get; }

        protected IBindingContext BindingContext { get; set; }

        public ExpressionEqualityComparer ExpressionEquality { get; }

        protected SelectExpression Select { get; set; }

        protected static long VariablesCounter = 0;

        const string VarPrefix = "_var_";

        public ModelBinder(IQueryContext queryContext)
        {
            QueryContext = queryContext;
            ExpressionEquality = new ExpressionEqualityComparer();
        }

        public Delegate Bind<TEntity>(IBindingContext bindingContext)
        {
            var model = bindingContext.Expression.As<SelectExpression>()?.QueryModel?.Model ?? bindingContext.Expression;
            var select = bindingContext.Expression;

            if (select is AggregateExpression aggreate)
            {
                Select = aggreate.KeySelector?.As<ColumnExpression>()?.Select;
            }
            else if (select.Is<AllExpression>())
            {
                Select = select.As<AllExpression>().Select.As<SelectExpression>();
            }
            else if (select.Is<AnyExpression>())
            {
                Select = select.As<AnyExpression>().Select.As<SelectExpression>();
            }
            else
            {
                Select = select.As<SelectExpression>();
            }

            BindingContext = bindingContext;

            var modelCreation = Visit(model);
            return Expression.Lambda(modelCreation, DataReader).Compile();
        }

        public override Expression Visit(Expression expression)
        {
            //子查询
            var queryModel = QueryContext.ModelMapper.GetValue(expression);
            if (queryModel != null &&
                queryModel != Select.QueryModel &&
                queryModel.ModelType.IsCollection())
            {
                return CreateSubQueryModel(queryModel);
            }

            if (queryModel != null &&
                queryModel != Select.QueryModel &&
                queryModel.RefrencedColumns.Count != 0)
            {
                return CreateSubQueryModel(queryModel);
            }

            var tryGetMaped = expression.Is<AggregateExpression>() ? expression : Select.Mapper.GetValue(expression);
            if (tryGetMaped != null)
            {
                var bind = BindQueryProjection(tryGetMaped);
                if (bind == null)
                {
                    bind = BindQueryProjection(tryGetMaped);
                    if (bind == null)
                    {
                        var mapped2 = Select.Mapper.GetValue(tryGetMaped);
                        if (mapped2 != null)
                        {
                            tryGetMaped = mapped2;
                        }

                        bind = BindQueryProjection(tryGetMaped);
                    }
                }

                if (bind == null)
                {
                    throw new Exception();
                }

                return bind;
            }

            return base.Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count == 0)
            {
                return Visit(methodCall.Object);
            }

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            for (var i = 0; i < Select.Projections.Count; i++)
            {
                var projection = Select.Projections[i];
                if (projection.Is<AggregateExpression>())
                {
                    projection = projection.As<AggregateExpression>().KeySelector;
                }

                if (projection.Is<AliasExpression>())
                {
                    projection = projection.As<AliasExpression>().Expression;
                }

                if (ExpressionEquality.Equals(projection, constant))
                {
                    var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(constant.Type);
                    return Expression.Call(null, valueSetter, DataReader, Expression.Constant(i));
                }
            }

            return constant;
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            var queryModel = QueryContext.ModelMapper.GetValue(member.Expression) ?? QueryContext.ModelMapper.GetValue(member);
            var bind = GetMemberBindingExpression(queryModel, member.Member);
            if (bind == null)
            {
                return Visit(member.Expression);
            }

            return BindQueryProjection(bind);
        }

        protected override Expression VisitMemberInit(MemberInitExpression memInit)
        {
            var eNewBlock = Visit(memInit.NewExpression) as BlockExpression;
            var mes = new List<MemberExpressionPair>();

            foreach (var binding in memInit.Bindings.OfType<MemberAssignment>())
            {
                var bind = BindMember(memInit, binding.Member, binding.Expression);

                mes.Add(new MemberExpressionPair(binding.Member, bind));
            }

            return BindMembers(eNewBlock, mes);
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            var binds = new List<Expression>();

            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                var bind = BindMember(newExpression, newExpression.Members[i], newExpression.Arguments[i]);

                binds.Add(bind);
            }

            var eBlock = CreateEntityNewExpressionBlock(newExpression.Constructor, newExpression.Type, binds);

            return eBlock;
        }

        protected virtual Expression BindQueryProjection(Expression projection)
        {
            for (var i = 0; i < Select.Projections.Count; i++)
            {
                var mappedProjection = Select.Mapper.GetValue(projection);

                if (ExpressionEquality.Equals(Select.Projections[i], projection) ||
                    ExpressionEquality.Equals(Select.Projections[i], mappedProjection))
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
                if (queryModel.Model.Type != member.DeclaringType)
                {
                    queryModel = queryModel.Previous;
                }

                var memberExpression = Expression.MakeMemberAccess(queryModel.Model, member);
                var projection = QueryContext.BindingMaper.GetValue(memberExpression);

                if (projection == null)
                {
                    queryModel = queryModel?.Previous;
                    continue;
                }

                return projection;
            }

            return null;
        }

        protected virtual BlockExpression BindMembers(BlockExpression eNewBlock, List<MemberExpressionPair> memberExpressions)
        {
            var eObject = eNewBlock.Variables.FirstOrDefault();
            var blockVars = new List<ParameterExpression>(eNewBlock.Variables);
            var memBindings = new List<Expression>();

            foreach (var memberExpression in memberExpressions)
            {
                memBindings.Add(Expression.Assign(
                    Expression.MakeMemberAccess(eObject, memberExpression.Member),
                    memberExpression.Expression));
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

        protected virtual Expression BindMember(Expression modelTemplate, MemberInfo member, Expression memberExpression)
        {
            var queryModel = Select.QueryModel.FindQueryModel(modelTemplate);
            var binding = memberExpression.NodeType == ExpressionType.Extension ? memberExpression : GetMemberBindingExpression(queryModel, member);

            if (!memberExpression.Type.IsPrimtiveType())
            {
                binding = binding is SelectExpression query
                       ? CreateSubQueryModel(query.QueryModel)
                       : Visit(memberExpression);

                return binding;
            }

            //此处是简单类型,应该是聚合,Select(item)
            binding = binding is SelectExpression
            ? Visit(memberExpression)
            : BindQueryProjection(binding);

            if (binding == null)
            {
                throw new Exception("projection not found!");
            }

            return binding;
        }

        protected virtual Expression CreateSubQueryModel(QueryEntityModel queryModel)
        {
            var modelElementType = queryModel.ModelElementType;
            var modelType = typeof(EntityPropertyEnumerable<>).MakeGenericType(modelElementType);
            var modelEleNew = Expression.New(
                        modelType.GetConstructor(new Type[] { typeof(Expression), typeof(IQueryContext), typeof(IQueryExecutor) }),
                        Expression.Constant(queryModel.Select),
                        Expression.Constant(QueryContext),
                        Expression.Constant(BindingContext.QueryExecutor));

            Expression model = Expression.Convert(Expression.Call(
                    null,
                    GetOrSetMemberValueMethod,
                    Expression.Constant(QueryContext.QueryValueCache),
                    Expression.Constant(queryModel),
                    modelEleNew),
                    typeof(IEnumerable<>).MakeGenericType(modelElementType)
                );

            model = FilterSubQuery(queryModel, model);

            model = Expression.Call(
                null,
                QueryEnumerable.ToListMethod.MakeGenericMethod(modelElementType),
                model);

            if (modelElementType == queryModel.ModelType)
            {
                return Expression.Call(
                    null,
                    QueryEnumerable.FirstOrDefaultMethod.MakeGenericMethod(modelElementType),
                    model);
            }

            return model;
        }

        protected virtual Expression FilterSubQuery(QueryEntityModel subQueryModel, Expression subQueryObj)
        {
            if (subQueryModel.RefrencedColumns.Count == 0)
            {
                return subQueryObj;
            }

            var propertyModel = Expression.Parameter(subQueryModel.RefrencedColumns.FirstOrDefault().Member.DeclaringType);
            var predicate = null as Expression;

            foreach (var item in subQueryModel.RefrencedColumns)
            {
                var projection = BindQueryProjection(item.RefrencedColumn);
                if (projection == null)
                {
                    continue;
                }

                var relation = Expression.Equal(projection, Expression.MakeMemberAccess(propertyModel, item.Member));
                if (predicate == null)
                {
                    predicate = relation;
                }
                else
                {
                    predicate = Expression.AndAlso(relation, predicate);
                }
            }

            if (predicate == null)
            {
                return subQueryObj;
            }

            var convert = typeof(Enumerable).GetMethod("OfType").MakeGenericMethod(propertyModel.Type);

            return Expression.Call(
                null,
                ReflectionUtil.EnumerableWhere.MakeGenericMethod(propertyModel.Type),
                Expression.Call(null, convert, subQueryObj),
                Expression.Lambda(predicate, propertyModel));
        }

        /// <summary>
        /// {
        ///     var entity=new Entity();
        ///     return entity;
        /// }
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static BlockExpression CreateEntityNewExpressionBlock(
            ConstructorInfo constructor,
            Type modelType,
            IEnumerable<Expression> constructorArguments)
        {
            if (constructor == null)
            {
                throw new NotImplementedException($"Type:{modelType.FullName} need a conscrutor which is none of parameters!");
            }

            var ctorArgs = new List<Expression>();
            var propertyValues = new List<Expression>();
            var propertyDeclares = new List<ParameterExpression>();

            foreach (var argument in constructorArguments ?? new List<Expression>())
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

            var beginOfBlock = Expression.Label(modelType, VarPrefix + VariablesCounter++);
            var varOfEntity = Expression.Variable(modelType, VarPrefix + VariablesCounter++);
            var valueOfEntity = Expression.New(constructor, constructorArguments == null ? null : ctorArgs);
            var setVarOfEntityValue = Expression.Assign(varOfEntity, valueOfEntity);
            var returnVarOfEntity = Expression.Return(beginOfBlock, varOfEntity);
            var endOfBlock = Expression.Label(beginOfBlock, varOfEntity);

            propertyDeclares.Add(varOfEntity);
            propertyValues.Add(setVarOfEntityValue);
            propertyValues.Add(returnVarOfEntity);
            propertyValues.Add(endOfBlock);

            return Expression.Block(propertyDeclares, propertyValues.ToArray());
        }

        public static object GetOrAddSubQueryValue(IQueryValueCache valueCache, QueryEntityModel queryModel, object value)
        {
            var v = valueCache.GetValue(queryModel);
            if (v == null)
            {
                valueCache.SetValue(queryModel, value);
                return value;
            }

            return v;
        }

        public static MethodInfo GetOrSetMemberValueMethod = typeof(ModelBinder).GetMethod(nameof(GetOrAddSubQueryValue));
    }
}