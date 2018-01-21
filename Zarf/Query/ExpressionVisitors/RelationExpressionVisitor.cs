using System.Collections.Generic;
using System.Linq.Expressions;

using Zarf.Extensions;
using Zarf.Query.Expressions;
using System.Reflection.Emit;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;
using Zarf.Metadata.Entities;

namespace Zarf.Query.ExpressionVisitors
{
    internal class SubQueryModelTypeDescriptor
    {
        public Dictionary<string, ColumnExpression> FieldMaps { get; set; }

        public Type SubModelType { get; set; }
    }

    internal class SubQueryModelTypeCache
    {
        public Type ModelType { get; set; }

        public Dictionary<string, Type> Fields { get; set; }
    }

    internal static class SubQueryModelTypeGenerator
    {
        private static ConcurrentDictionary<string, SubQueryModelTypeCache> ModelTypeCaches { get; }

        private static ModuleBuilder ModuleBuilder { get; }

        static SubQueryModelTypeGenerator()
        {
            ModelTypeCaches = new ConcurrentDictionary<string, SubQueryModelTypeCache>();

            ModuleBuilder = AssemblyBuilder
                .DefineDynamicAssembly(new AssemblyName("__SubQueryExtension__"), AssemblyBuilderAccess.RunAndCollect)
                .DefineDynamicModule("__SubQueryExtension__Module__");
        }

        public static SubQueryModelTypeDescriptor GenRealtionType(Type modelType, List<ColumnExpression> relatedColumns)
        {
            if (modelType.IsSealed)
            {
                throw new Exception("sealed type not supported filter in an subquery!");
            }

            var typeName = modelType.Name + relatedColumns
                .Select(item => item.Type.GetHashCode())
                .Concat(new[] { modelType.GetHashCode() })
                .Aggregate((hashCode, t) => hashCode + (hashCode * 37 ^ t));

            var typeCache = ModelTypeCaches.GetOrAdd(typeName, (key) =>
            {
                var fields = new Dictionary<string, Type>();
                var typeBuilder = ModuleBuilder.DefineType(
                    typeName,
                    TypeAttributes.Class,
                    modelType);

                for (var i = 0; i < relatedColumns.Count; i++)
                {
                    fields["__RealtionId__" + i] = relatedColumns[i].Type;
                    typeBuilder.DefineField("__RealtionId__" + i, relatedColumns[i].Type, FieldAttributes.Public | FieldAttributes.SpecialName);
                }

                return new SubQueryModelTypeCache
                {
                    ModelType = typeBuilder.CreateType(),
                    Fields = fields
                };
            });

            var fieldsMap = new Dictionary<string, ColumnExpression>();
            var fieldNames = new HashSet<string>();

            for (var i = 0; i < relatedColumns.Count; i++)
            {
                foreach (var item in typeCache.Fields.Where(kv => !fieldNames.Contains(kv.Key)))
                {
                    if (relatedColumns[i].Type == item.Value)
                    {
                        fieldsMap[item.Key] = relatedColumns[i];
                        fieldNames.Add(item.Key);
                        break;
                    }
                }
            }

            return new SubQueryModelTypeDescriptor()
            {
                FieldMaps = fieldsMap,
                SubModelType = typeCache.ModelType
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
            var queryModel = QueryContext.QueryModelMapper.GetQueryModel(mem.Expression);
            if (queryModel != null)
            {
                var property = Expression.MakeMemberAccess(queryModel.Model, mem.Member);
                var propertyExpression = QueryContext.MemberBindingMapper.GetMapedExpression(property);
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

            if (node.Is<SelectExpression>())
            {
                node.As<SelectExpression>().IsPartOfPredicate = true;
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
            var select = null as SelectExpression;
            if (binary.Left.Is<SelectExpression>() && binary.Right.IsNullValueConstant())
            {
                select = binary.Left.As<SelectExpression>();
            }
            else if (binary.Right.Is<SelectExpression>() && binary.Left.IsNullValueConstant())
            {
                select = binary.Right.As<SelectExpression>();
            }

            if (select != null)
            {
                select.Where = new WhereExperssion(Visit(select.Where.Predicate));
                return Expression.Not(new ExistsExpression(select));
            }

            if (!binary.Left.Type.IsPrimtiveType())
            {
                throw new System.NotSupportedException($"not supported compared the value of {binary.Left.Type.Name}");
            }

            return base.Visit(binary);
        }

        protected virtual Expression VisitNotEqual(BinaryExpression binary)
        {
            var select = null as SelectExpression;
            if (binary.Left.Is<SelectExpression>() && binary.Right.IsNullValueConstant())
            {
                select = binary.Left.As<SelectExpression>();
            }
            else if (binary.Right.Is<SelectExpression>() && binary.Left.IsNullValueConstant())
            {
                select = binary.Right.As<SelectExpression>();
            }

            if (select != null)
            {
                select.Where = new WhereExperssion(Visit(select.Where.Predicate));
                return new ExistsExpression(select);
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
            all.Select.Where = new WhereExperssion(Visit(all.Select.Where.Predicate));
            return Expression.Not(new ExistsExpression(all.Select));
        }

        protected virtual Expression VisitAny(AnyExpression any)
        {
            any.Select.Where = new WhereExperssion(Visit(any.Select.Where.Predicate));
            return new ExistsExpression(any.Select);
        }
    }

    public class SubQueryModelRewriter : ExpressionVisitorBase
    {
        public List<ColumnExpression> RefrencedOuterColumns { get; }

        public SelectExpression Select { get; }

        public IQueryContext Context { get; }

        public SubQueryModelRewriter(SelectExpression select, IQueryContext context)
        {
            RefrencedOuterColumns = new List<ColumnExpression>();
            Context = context;
            Select = select;
        }

        public Expression ChangeQueryModel(Expression exp)
        {
            exp = Visit(exp);

            if (RefrencedOuterColumns.Count == 0) return exp;

            var modelTypeDescriptor = SubQueryModelTypeGenerator.GenRealtionType(Select.QueryModel.Model.Type, RefrencedOuterColumns);
            var modelNewType = modelTypeDescriptor.SubModelType;
            var model = Select.QueryModel.Model;
            var modelNewExpression = Select.QueryModel.Model.As<NewExpression>();
            List<MemberAssignment> bindings = null;

            if (modelNewExpression == null)
            {
                var memberInit = Select.QueryModel.Model.As<MemberInitExpression>();
                bindings = memberInit.Bindings.OfType<MemberAssignment>().ToList();
                modelNewExpression = Select.QueryModel.Model.As<MemberInitExpression>().NewExpression;
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
                bindings.Add(Expression.Bind(field, item.Value));
            }

            Select.QueryModel = new QueryEntityModel(Select, Select.QueryModel.Model, Select.QueryModel.ModelType, Select.QueryModel);

            modelNewExpression = CreateNewExpression(modelNewType, modelNewExpression.Arguments.ToList());
            model = Expression.MemberInit(modelNewExpression, bindings);

            Select.QueryModel = new QueryEntityModel(
                Select,
                model,
                Select.QueryModel.ModelType.GetGenericTypeDefinition().MakeGenericType(modelNewType),
                Select.QueryModel);

            foreach (var item in modelTypeDescriptor.FieldMaps)
            {
                var field = modelNewType.GetField(item.Key);
                Select.QueryModel.RefrencedColumns.Add(new QueryEntityModelRefrenceOuterColumn()
                {
                    Member = field,
                    RefrencedColumn = item.Value
                });
            }

            Select.Groups.Add(new GroupExpression(GetAllColumns(Select)));

            Context.QueryModelMapper.MapQueryModel(model, Select.QueryModel);

            return exp;
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
                    return extension;
            }
        }

        protected virtual Expression VisitColumn(ColumnExpression outer)
        {
            if (!Select.ContainsSelectExpression(outer.Select))
            {
                var query = outer.Select.Clone();
                var columnExpression = outer.Clone(Context.Alias.GetNewColumn());

                columnExpression.Select = query;

                outer.Select.AddProjection(columnExpression);

                Select.AddJoin(new JoinExpression(query, null, JoinType.Cross));

                Select.AddProjection(columnExpression);
                RefrencedOuterColumns.Add(columnExpression);
            }

            return outer;
        }

        protected IEnumerable<ColumnExpression> GetAllColumns(SelectExpression select)
        {
            //Visit
            foreach (var item in select.Projections)
            {
                if (item is AliasExpression alias && alias.Expression is ColumnExpression)
                {
                    yield return alias.Expression as ColumnExpression;
                }

                if (item is ColumnExpression column)
                {
                    yield return column;
                }

                if (item is AggregateExpression aggreate && aggreate.KeySelector is ColumnExpression)
                {
                    yield return aggreate.KeySelector as ColumnExpression;
                }
            }
        }

        protected virtual Expression VisitAlias(AliasExpression alias)
        {
            var expression = Visit(alias.Expression);
            if (expression != alias.Expression)
            {
                return new AliasExpression(alias.Alias, expression, alias.Source);
            }

            return alias;
        }

        protected virtual Expression VisitQuery(SelectExpression select)
        {
            var predicate = Visit(select.Where?.Predicate);
            if (predicate != select.Where?.Predicate)
            {
                select.Where = new WhereExperssion(predicate);
            }

            return select;
        }

        protected virtual Expression VisitAggregate(AggregateExpression aggreatge)
        {
            Visit(aggreatge.KeySelector);
            return aggreatge;
        }

        protected virtual Expression VisitAll(AllExpression all)
        {
            Visit(all.Select);
            return all;
        }

        protected virtual Expression VisitAny(AnyExpression any)
        {
            Visit(any.Select);
            return any;
        }
    }
}
