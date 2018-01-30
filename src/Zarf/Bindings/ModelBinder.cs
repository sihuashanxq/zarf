using System;
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

        /// <summary>
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override Expression Visit(Expression expression)
        {
            //子查询
            var queryModel = QueryContext.ModelMapper.GetValue(expression);
            if (queryModel != null &&
                queryModel != Select.QueryModel &&
                queryModel.ModelType.IsCollection())
            {
                return CreateSubQueryModelExpression(queryModel);
            }

            if (queryModel != null &&
                queryModel != Select.QueryModel &&
                queryModel.RefrencedOuterColumns.Count != 0)
            {
                return CreateSubQueryModelExpression(queryModel);
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

        /// <summary>
        /// 处理方法调用
        /// </summary>
        /// <param name="methodCall"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count == 0)
            {
                return Visit(methodCall.Object);
            }

            return base.VisitMethodCall(methodCall);
        }

        /// <summary>
        /// 处理查询中的常量
        /// </summary>
        /// <param name="constant"></param>
        /// <returns></returns>
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

        /// <summary>
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
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

        /// <summary>
        /// new User{ Id=1}
        /// </summary>
        /// <param name="memInit"></param>
        /// <returns></returns>
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

        /// <summary>
        /// new {}
        /// </summary>
        /// <param name="newExpression"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 绑定查询投影中的某列
        /// </summary>
        /// <param name="projection"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取类型成员绑定表达式
        /// </summary>
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

        /// <summary>
        /// 绑定类型成员
        /// </summary>
        /// <param name="eNewBlock"></param>
        /// <param name="memberExpressions"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 绑定类型成员
        /// </summary>
        /// <param name="modelTemplate">类型模板</param>
        /// <param name="member">成员</param>
        /// <param name="memberExpression">成员对应的表达式</param>
        /// <returns></returns>
        protected virtual Expression BindMember(Expression modelTemplate, MemberInfo member, Expression memberExpression)
        {
            var queryModel = Select.QueryModel.FindQueryModel(modelTemplate);
            var binding = memberExpression.NodeType == ExpressionType.Extension ? memberExpression : GetMemberBindingExpression(queryModel, member);

            if (!memberExpression.Type.IsPrimtiveType())
            {
                binding = binding is SelectExpression query
                       ? CreateSubQueryModelExpression(query.QueryModel)
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

        /// <summary>
        /// 创建子查询实例化表达式
        /// </summary>
        protected virtual Expression CreateSubQueryModelExpression(QueryEntityModel queryModel)
        {
            var modelElementType = queryModel.ModelElementType;
            var modelType = typeof(EntityEnumerable<>).MakeGenericType(modelElementType);
            var modelEleNew = Expression.New(
                        modelType.GetConstructor(new Type[] { typeof(Expression), typeof(IQueryExecutor), typeof(IQueryContext) }),
                        Expression.Constant(queryModel.Select),
                        Expression.Constant(BindingContext.QueryExecutor),
                        Expression.Constant(QueryContext)
                     );

            Expression model = Expression.Convert(Expression.Call(
                    null,
                    GetOrSetMemberValueMethod,
                    Expression.Constant(QueryContext.QueryValueCache),
                    Expression.Constant(queryModel),
                    modelEleNew),
                    typeof(IEnumerable<>).MakeGenericType(modelElementType)
                );

            model = QueryModelFilter(queryModel, model);

            if (modelElementType == queryModel.ModelType)
            {
                return Expression.Call(
                    null,
                    QueryEnumerable.FirstOrDefaultMethod.MakeGenericMethod(modelElementType),
                    model);
            }

            if (!queryModel.ModelType.IsGenericType || typeof(IEnumerable<>) != queryModel.ModelType.GetGenericTypeDefinition())
            {
                model = Expression.Call(
                    null,
                    QueryEnumerable.ToListMethod.MakeGenericMethod(modelElementType),
                    model);
            }

            return model;
        }

        /// <summary>
        /// 子查询QueryModel内存过滤
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="modelObj"></param>
        /// <returns></returns>
        protected virtual Expression QueryModelFilter(QueryEntityModel queryModel, Expression modelObj)
        {
            if (queryModel.RefrencedOuterColumns.Count == 0)
            {
                return modelObj;
            }

            var propertyModel = Expression.Parameter(queryModel.RefrencedOuterColumns.FirstOrDefault().Member.DeclaringType);
            var predicate = null as Expression;

            var varies = new List<ParameterExpression>();
            var blocks = new List<Expression>();

            foreach (var item in queryModel.RefrencedOuterColumns)
            {
                var projection = BindQueryProjection(item.RefrencedColumn);
                if (projection == null)
                {
                    continue;
                }

                //关联字段值预求值,放入局部变量,避免主查询连接失效后,子查询过滤失败
                var relationVar = Expression.Variable(projection.Type, VarPrefix + VariablesCounter++);
                var realtionFieldValue = Expression.Assign(relationVar, projection);
                var relation = Expression.Equal(relationVar, Expression.MakeMemberAccess(propertyModel, item.Member));

                varies.Add(relationVar);
                blocks.Add(realtionFieldValue);
                predicate = predicate == null ? relation : Expression.AndAlso(relation, predicate);
            }

            if (predicate == null)
            {
                return modelObj;
            }

            var convert = typeof(Enumerable).GetMethod("OfType").MakeGenericMethod(propertyModel.Type);
            var model = Expression.Call(
               null,
               ReflectionUtil.EnumerableWhere.MakeGenericMethod(propertyModel.Type),
               Expression.Call(null, convert, modelObj),
               Expression.Lambda(predicate, propertyModel));

            //关联字段值预求值,放入局部变量,避免主查询连接失效后,子查询过滤失败
            var label = Expression.Label(modelObj.Type, VarPrefix + VariablesCounter++);
            var modelVar = Expression.Variable(modelObj.Type, VarPrefix + VariablesCounter++);
            var modelValue = Expression.Assign(modelVar, model);
            var ret = Expression.Return(label, modelVar);
            var end = Expression.Label(label, modelVar);

            varies.Insert(0, modelVar);
            blocks.AddRange(new Expression[] { modelValue, ret, end });

            return Expression.Block(varies.ToArray(), blocks.ToArray());
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

            var args = new List<Expression>();
            var values = new List<Expression>();            //赋值语句,return
            var varies = new List<ParameterExpression>();   //变量定义

            foreach (var item in constructorArguments ?? new List<Expression>())
            {
                if (item.NodeType != ExpressionType.Block)
                {
                    args.Add(item);
                    continue;
                }

                var block = item.As<BlockExpression>();
                args.Add(block.Variables[0]);
                varies.AddRange(block.Variables.ToList());
                values.AddRange(block.Expressions.Take(block.Expressions.Count - 2));
            }

            var label = Expression.Label(modelType, VarPrefix + VariablesCounter++);
            var modelVar = Expression.Variable(modelType, VarPrefix + VariablesCounter++);
            var modelValue = Expression.New(constructor, constructorArguments == null ? null : args);
            var setModelVar = Expression.Assign(modelVar, modelValue);
            var ret = Expression.Return(label, modelVar);
            var end = Expression.Label(label, modelVar);

            varies.Add(modelVar);
            values.Add(setModelVar);
            values.Add(ret);
            values.Add(end);

            return Expression.Block(varies, values.ToArray());
        }

        /// <summary>
        /// 缓存子查询实例
        /// </summary>
        /// <param name="valueCache"></param>
        /// <param name="queryModel"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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
