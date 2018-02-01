using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Metadata.Entities;
using Zarf.Query.Expressions;
using Zarf.Query.Internals.ModelTypes;

namespace Zarf.Query.Visitors
{
    /// <summary>
    /// QueryModel 重写
    /// 引用了外部列
    /// </summary>
    public class QueryModelRewriterExpressionVisitor : ExpressionVisitorBase
    {
        /// <summary>
        /// 外部关联列
        /// </summary>
        public List<ColumnExpression> RelationColumns { get; }

        public SelectExpression Select { get; }

        public IQueryContext Context { get; }

        public QueryModelRewriterExpressionVisitor(SelectExpression select, IQueryContext context)
        {
            RelationColumns = new List<ColumnExpression>();
            Context = context;
            Select = select;
        }

        public Expression ChangeQueryModel(Expression exp)
        {
            exp = Visit(exp);

            if (RelationColumns.Count == 0) return exp;

            var modelTypeDescriptor = QueryModelTypeGenerator.GenRealtionType(Select.QueryModel.Model.Type, RelationColumns);
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
                Select.QueryModel.RefrencedOuterColumns.Add(new QueryEntityModelRefrenceOuterColumn()
                {
                    Member = field,
                    RefrencedColumn = item.Value
                });
            }

            Select.Groups.Add(new GroupExpression(GetColumns(Select).ToList()));

            Context.ModelMapper.Map(model, Select.QueryModel);

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

        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            if (Select.ContainsSelectExpression(column.Select))
            {
                return column;
            }
            var relationSelect = column.Select;
            var relationColumnSelect = relationSelect.Clone();

            var columnExpression = new ColumnExpression(
                relationColumnSelect,
                column.Column,
                column.Type,
                Context.AliasGenerator.GetNewColumn());

            RelationColumns.Add(columnExpression);

            relationColumnSelect.AddProjection(columnExpression);
            relationSelect.AddProjection(columnExpression);

            Select.AddJoin(new JoinExpression(relationColumnSelect, null, JoinType.Cross));

            if (columnExpression.Select.CanJoinAsFlatTable())
            {
                Select.AddProjection(columnExpression);
                return column;
            }

            //不能平坦方式Join,此时引用的列在一个子查询中
            //引用其别名
            var aliasColumn = new ColumnExpression(relationColumnSelect, new Column(columnExpression.Alias), columnExpression.Type, string.Empty);
            Select.Mapper.Map(columnExpression, aliasColumn);
            Select.AddProjection(aliasColumn);
            return aliasColumn;
        }

        protected IEnumerable<ColumnExpression> GetColumns(SelectExpression select)
        {
            //Visit
            foreach (var item in select.Projections)
            {
                if (item is AliasExpression alias && alias.Expression is ColumnExpression)
                {
                    yield return (alias.Expression as ColumnExpression).Clone(string.Empty);
                }

                if (item is ColumnExpression column)
                {
                    yield return column.Clone(string.Empty);
                }

                if (item is AggregateExpression aggreate && aggreate.KeySelector is ColumnExpression)
                {
                    yield return (aggreate.KeySelector as ColumnExpression).Clone(string.Empty);
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
