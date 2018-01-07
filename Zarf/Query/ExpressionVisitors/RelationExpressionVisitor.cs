using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using System.Reflection.Emit;
using System;
using System.Linq;
using System.Reflection;

namespace Zarf.Query.ExpressionVisitors
{
    public class SubQueryModelTypeDescriptor
    {
        public Dictionary<string, ColumnExpression> FieldMaps { get; set; }

        public Type SubModelType { get; set; }
    }

    public static class SubQueryModelTypeGenerator
    {
        public static SubQueryModelTypeDescriptor GenRealtionType(Type modelType, List<ColumnExpression> relatedColumns)
        {
            //var constructor = modelType.GetConstructors()[0];
            var fieldMaps = new Dictionary<string, ColumnExpression>();

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("__SubQueryExtension__"), AssemblyBuilderAccess.RunAndCollect);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule("__SubQueryExtension__Module__");
            var typeBuilder = moduleBuilder.DefineType(
                modelType.Name + relatedColumns.GetHashCode(),
                TypeAttributes.Class,
                modelType);

            //var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructor.GetParameters().Select(i => i.ParameterType).ToArray());
            //var il = ctorBuilder.GetILGenerator();
            //for (var i = 0; i < constructor.GetParameters().Length; i++)
            //{
            //    il.Emit(OpCodes.Ldarg, i);
            //}

            //il.Emit(OpCodes.Call, constructor);
            //il.Emit(OpCodes.Ret);

            for (var i = 0; i < relatedColumns.Count; i++)
            {
                var filedName = "__RealtionId__" + i;

                fieldMaps[filedName] = relatedColumns[i];

                typeBuilder.DefineField(filedName, relatedColumns[i].Type, FieldAttributes.Public | FieldAttributes.SpecialName);
            }

            var subModelType = typeBuilder.CreateType();

            return new SubQueryModelTypeDescriptor()
            {
                FieldMaps = fieldMaps,
                SubModelType = subModelType
            };
        }
    }

    /// <summary>
    /// 查询条件转换
    /// </summary>
    public class RelationExpressionCompiler : QueryCompiler
    {
        public RelationExpressionCompiler(IQueryContext context) : base(context)
        {

        }

        public override Expression Compile(Expression query)
        {
            if (query == null)
            {
                return query;
            }

            if (query.NodeType == ExpressionType.MemberAccess)
            {
                return VisitMember(query.As<MemberExpression>());
            }

            return base.Compile(query);
        }

        protected override Expression VisitMember(MemberExpression mem)
        {
            var queryModel = Context.QueryModelMapper.GetQueryModel(mem.Expression);
            if (queryModel != null)
            {
                var property = Expression.MakeMemberAccess(queryModel.Model, mem.Member);
                var propertyExpression = Context.MemberBindingMapper.GetMapedExpression(property);
                if (propertyExpression.NodeType != ExpressionType.Extension)
                {
                    propertyExpression = base.Visit(propertyExpression);
                }

                if (propertyExpression.Is<AliasExpression>())
                {
                    return propertyExpression.As<AliasExpression>().Expression;
                }

                return propertyExpression;
            }

            return base.Compile(mem);
        }
    }

    public class RelationExpressionVisitor : ExpressionVisitorBase
    {
        public override Expression Visit(Expression node)
        {
            if (node.Is<AllExpression>())
            {
                return VisitAll(node.As<AllExpression>());
            }

            if (node.Is<AnyExpression>())
            {
                return VisitAny(node.As<AnyExpression>());
            }

            if (node.Is<QueryExpression>())
            {
                node.As<QueryExpression>().IsPartOfPredicate = true;
            }

            if (node is AliasExpression alias)
            {
                return alias.Expression;
            }

            if (node.NodeType == ExpressionType.Extension)
            {
                return node;
            }

            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    return VisitEqual(node.As<BinaryExpression>());
                case ExpressionType.NotEqual:
                    return VisitNotEqual(node.As<BinaryExpression>());
                case ExpressionType.Or:
                    return VisitOr(node.As<BinaryExpression>());
                case ExpressionType.And:
                    return VisitAnd(node.As<BinaryExpression>());
            }

            return base.Visit(node);
        }

        protected virtual Expression VisitEqual(BinaryExpression binary)
        {
            var query = null as QueryExpression;
            if (binary.Left.Is<QueryExpression>() && binary.Right.IsNullValueConstant())
            {
                query = binary.Left.As<QueryExpression>();
            }
            else if (binary.Right.Is<QueryExpression>() && binary.Left.IsNullValueConstant())
            {
                query = binary.Right.As<QueryExpression>();
            }

            if (query != null)
            {
                query.Where = new WhereExperssion(Visit(query.Where.Predicate));
                return Expression.Not(new ExistsExpression(query));
            }

            if (!binary.Left.Type.IsPrimtiveType())
            {
                throw new System.NotSupportedException($"not supported compared the value of {binary.Left.Type.Name}");
            }

            return base.Visit(binary);
        }

        protected virtual Expression VisitNotEqual(BinaryExpression binary)
        {
            var query = null as QueryExpression;
            if (binary.Left.Is<QueryExpression>() && binary.Right.IsNullValueConstant())
            {
                query = binary.Left.As<QueryExpression>();
            }
            else if (binary.Right.Is<QueryExpression>() && binary.Left.IsNullValueConstant())
            {
                query = binary.Right.As<QueryExpression>();
            }

            if (query != null)
            {
                query.Where = new WhereExperssion(Visit(query.Where.Predicate));
                return new ExistsExpression(query);
            }

            if (!ReflectionUtil.SimpleTypes.Contains(binary.Left.Type))
            {
                throw new System.NotSupportedException($"not supported compared the value of {binary.Left.Type.Name}");
            }

            return base.Visit(binary);
        }

        protected virtual Expression VisitOr(BinaryExpression binary)
        {
            if (binary.Left.Type != typeof(bool))
            {
                return base.Visit(binary);
            }

            var left = Visit(binary.Left);
            var right = Visit(binary.Right);
            if (!left.Is<ExistsExpression>() && !left.Is<BinaryExpression>())
            {
                left = Expression.Equal(left, Expression.Constant(true));
            }

            if (!right.Is<ExistsExpression>() && !right.Is<BinaryExpression>())
            {
                right = Expression.Equal(right, Expression.Constant(true));
            }

            return Expression.OrElse(left, right);
        }

        protected virtual Expression VisitAnd(BinaryExpression binary)
        {
            if (binary.Left.Type != typeof(bool))
            {
                return base.Visit(binary);
            }

            var left = Visit(binary.Left);
            var right = Visit(binary.Right);
            if (!left.Is<ExistsExpression>() && !left.Is<BinaryExpression>())
            {
                left = Expression.Equal(left, Expression.Constant(true));
            }

            if (!right.Is<ExistsExpression>() && !right.Is<BinaryExpression>())
            {
                right = Expression.Equal(right, Expression.Constant(true));
            }

            return Expression.AndAlso(left, right);
        }

        protected virtual Expression VisitAll(AllExpression all)
        {
            all.Query.Where = new WhereExperssion(Visit(all.Query.Where.Predicate));
            return Expression.Not(new ExistsExpression(all.Query));
        }

        protected virtual Expression VisitAny(AnyExpression any)
        {
            any.Query.Where = new WhereExperssion(Visit(any.Query.Where.Predicate));
            return new ExistsExpression(any.Query);
        }
    }

    public class SubQueryModelRewriter : ExpressionVisitorBase
    {
        public List<ColumnExpression> RefrencedOuterColumns { get; }

        public QueryExpression Query { get; }

        public IQueryContext Context { get; }

        public SubQueryModelRewriter(QueryExpression query, IQueryContext context)
        {
            RefrencedOuterColumns = new List<ColumnExpression>();
            Context = context;
            Query = query;
        }

        public void ChangeQueryModel(Expression exp)
        {
            Visit(exp);

            if (RefrencedOuterColumns.Count == 0) return;

            var modelTypeDescriptor = SubQueryModelTypeGenerator.GenRealtionType(Query.QueryModel.Model.Type, RefrencedOuterColumns);
            var modelNewType = modelTypeDescriptor.SubModelType;
            var model = Query.QueryModel.Model;
            var modelNewExpression = Query.QueryModel.Model.As<NewExpression>();
            List<MemberAssignment> bindings = null;

            if (modelNewExpression == null)
            {
                var memberInit = Query.QueryModel.Model.As<MemberInitExpression>();
                bindings = memberInit.Bindings.OfType<MemberAssignment>().ToList();
                modelNewExpression = Query.QueryModel.Model.As<MemberInitExpression>().NewExpression;
            }

            if (modelNewExpression == null)
            {
                throw new Exception("error!");
            }

            if (bindings == null)
            {
                bindings = new List<MemberAssignment>();
            }

            foreach (var item in modelTypeDescriptor.FieldMaps)
            {
                var field = modelNewType.GetField(item.Key);
                //可能查询中没有值
                item.Value.Query.AddProjection(item.Value);
                bindings.Add(Expression.Bind(field, item.Value));
            }

            Query.QueryModel = new QueryEntityModel(Query, Query.QueryModel.Model, Query.QueryModel.ModelType, Query.QueryModel);

            modelNewExpression = CreateNewExpression(modelNewType, modelNewExpression.Arguments.ToList());
            model = Expression.MemberInit(modelNewExpression, bindings);

            Query.QueryModel = new QueryEntityModel(
                Query,
                model,
                Query.QueryModel.ModelType.GetGenericTypeDefinition().MakeGenericType(modelNewType),
                Query.QueryModel);

            foreach (var item in modelTypeDescriptor.FieldMaps)
            {
                var field = modelNewType.GetField(item.Key);
                Query.QueryModel.RefrencedColumns.Add(new QueryEntityModelRefrenceOuterColumn()
                {
                    Member = field,
                    RefrencedColumn = item.Value
                });
            }

            Context.QueryModelMapper.MapQueryModel(model, Query.QueryModel);
        }

        protected virtual NewExpression CreateNewExpression(Type modelType, List<Expression> arguments)
        {
            var constructor = modelType.GetConstructor(arguments.Select(item => item.Type).ToArray());
            return Expression.New(constructor, arguments);
        }

        protected override Expression VisitExtension(Expression extension)
        {
            switch (extension)
            {
                case ColumnExpression column:
                    return VisitColumn(column);
                case AggregateExpression aggreate:
                    return VisitAggregate(aggreate);
                case AliasExpression alias:
                    return VisitAlias(alias);
                case AnyExpression any:
                    return VisitAny(any);
                case AllExpression all:
                    return VisitAll(all);
                default:
                    throw new Exception("");
            }
        }

        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            if (!Query.ConstainsQuery(column.Query) && !column.Query.ConstainsQuery(Query))
            {
                RefrencedOuterColumns.Add(column);
                Query.AddProjection(column);
                Query.AddJoin(new JoinExpression(column.Query, null, JoinType.Cross));
            }

            return column;
        }

        protected virtual Expression VisitAlias(AliasExpression alias)
        {
            Visit(alias.Expression);
            return alias;
        }

        protected virtual Expression VisitQuery(QueryExpression query)
        {
            Visit(query.Where?.Predicate);
            return query;
        }

        protected virtual Expression VisitAggregate(AggregateExpression aggreatge)
        {
            Visit(aggreatge.KeySelector);
            return aggreatge;
        }

        protected virtual Expression VisitAll(AllExpression all)
        {
            Visit(all.Query);
            return all;
        }

        protected virtual Expression VisitAny(AnyExpression any)
        {
            Visit(any.Query);
            return any;
        }
    }
}
