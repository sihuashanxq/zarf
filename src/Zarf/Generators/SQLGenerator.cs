using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Zarf.Extensions;
using Zarf.Metadata.Entities;
using Zarf.Query.Expressions;
using Zarf.Update.Expressions;

namespace Zarf.Generators
{
    public abstract class SQLGenerator : ExpressionVisitor, ISQLGenerator
    {
        /// <summary>
        /// 参数计数
        /// </summary>
        private int _parameterCounter = 0;

        /// <summary>
        /// 生成的参数
        /// </summary>
        private List<DbParameter> _parameters;

        /// <summary>
        /// SQL
        /// </summary>
        protected StringBuilder SQL { get; set; }

        public virtual string Generate(Expression expression, List<DbParameter> parameters)
        {
            lock (this)
            {
                _parameterCounter = 0;

                SQL = new StringBuilder();
                _parameters = parameters;

                BuildExpression(expression);

                return SQL.ToString().Replace("[dbo].", "");
            }
        }

        public virtual string Generate(Expression expression)
        {
            return Generate(expression, new List<DbParameter>());
        }

        protected DbParameter CreateParameter(object parameterValue)
        {
            return new DbParameter("@P" + _parameterCounter++, parameterValue);
        }

        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            if (column.Select != null && !column.Select.Alias.IsNullOrEmpty())
            {
                SQL.Append(column.Select.Alias.Escape());
                SQL.Append('.');
            }

            if (column.Column == null)
            {
                Append(" NULL ");
            }
            else
            {
                SQL.Append(column.Column.Name.Escape());
            }

            if (!column.Alias.IsNullOrEmpty())
            {
                Append(" AS ");
                SQL.Append(column.Alias.Escape());
            }

            return column;
        }

        protected virtual Expression VisitAlias(AliasExpression alias)
        {
            if (alias.Expression is SelectExpression)
            {
                Append("( ");
                BuildExpression(alias.Expression);
                Append(" ) ");
            }
            else
            {
                BuildExpression(alias.Expression);
            }

            Append(" AS ", alias.Alias);
            return alias;
        }

        protected virtual Expression VisitExcept(ExceptExpression except)
        {
            Append(" Except ");
            BuildExpression(except.Select);
            return except;
        }

        protected virtual Expression VisitGroup(GroupExpression group)
        {
            BuildColumns(group.Columns);
            return group;
        }

        protected virtual Expression VisitIntersect(IntersectExpression intersec)
        {
            Append(" INTERSECT ");
            BuildExpression(intersec.Select);
            return intersec;
        }

        protected virtual Expression VisitUnion(UnionExpression union)
        {
            Append(" UNION ALL ");
            BuildExpression(union.Select);
            return union;
        }

        protected virtual Expression VisitJoin(JoinExpression join)
        {
            switch (join.JoinType)
            {
                case JoinType.Left:
                    Append(" Left JOIN ");
                    break;
                case JoinType.Right:
                    Append(" Right JOIN ");
                    break;
                case JoinType.Full:
                    Append(" Full JOIN ");
                    break;
                case JoinType.Inner:
                    Append(" Inner JOIN ");
                    break;
                case JoinType.Cross:
                    Append(" Cross JOIN ");
                    break;
            }

            if (join.Select.IsEmptyQuery() && join.Select.Projections.Count == 0)
            {
                BuildFromTable(join.Select);
            }
            else
            {
                BuildExpression(join.Select);
                Append("  AS " + join.Select.Alias.Escape());
            }

            if (join.JoinType != JoinType.Cross)
            {
                Append(" ON ");
                BuildExpression(join.Predicate ?? Utils.ExpressionTrue);
            }

            return join;
        }

        protected virtual Expression VisitOrder(OrderExpression order)
        {
            var direction = order.Direction == OrderDirection.Desc
                ? " DESC "
                : " ASC ";

            BuildColumns(order.Columns);
            SQL.Append(direction);
            return order;
        }

        protected virtual Expression VisitWhere(WhereExperssion where)
        {
            Append(" WHERE ");
            BuildExpression(where.Predicate);
            return where;
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            DbParameter parameter = null;

            if (constant.Type == typeof(bool))
            {
                parameter = CreateParameter(constant.Value.Cast<bool>() ? 1 : 0);
            }
            else if (constant.Type.IsPrimtiveType())
            {
                parameter = CreateParameter(constant.Value);
            }
            else
            {
                parameter = CreateParameter(constant.Value.ToString());
            }

            _parameters.Add(parameter);
            Append(parameter.Name);

            return constant;
        }

        protected override Expression VisitUnary(UnaryExpression unary)
        {
            switch (unary.NodeType)
            {
                case ExpressionType.Not:
                    Append(" NOT ( ");
                    if (unary.Operand.Is<ConstantExpression>() && unary.Operand.Type == typeof(bool))
                    {
                        Append(" 1 =");
                    }

                    BuildExpression(unary.Operand);
                    Append(" )");
                    break;
                default:
                    Visit(unary.Operand);
                    break;
            }

            return unary;
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            if (!Utils.OperatorMap.TryGetValue(binary.NodeType, out string op))
            {
                throw new NotImplementedException("ViistBinary");
            }

            var leftIsNull = binary.Left.IsNullValueConstant();
            var righIsNull = binary.Right.IsNullValueConstant();

            var left = binary.Left;
            var right = binary.Right;

            if (leftIsNull && righIsNull)
            {
                BuildExpression(Expression.Constant(1));
                SQL.Append(op);
                BuildExpression(Expression.Constant(1));

                return binary;
            }

            if (leftIsNull || righIsNull)
            {
                BuildExpression(leftIsNull ? right : left);
                if (binary.NodeType == ExpressionType.Equal)
                {
                    Append(" IS NULL ");
                    return binary;
                }

                if (binary.NodeType == ExpressionType.NotEqual)
                {
                    Append(" IS NOT NULL ");
                    return binary;
                }

                throw new ArgumentNullException(" NULL ");
            }

            BuildExpression(left);
            SQL.Append(op);
            BuildExpression(right);

            return binary;

        }

        protected virtual void BuildColumns(IEnumerable<ColumnExpression> columns)
        {
            foreach (var column in columns)
            {
                VisitColumn(column);
                Append(',');
            }

            SQL.Length--;
        }

        protected virtual void BuildLimit(SelectExpression select)
        {
            if (select.Limit != 0)
            {
                Append(" LIMIT  ", select.Limit, " ");
                return;
            }

            if (select.Offset != null)
            {
                Append(" LIMIT  ", int.MaxValue, " ");
            }
        }

        protected virtual void BuildDistinct(SelectExpression select)
        {
            if (select.IsDistinct)
            {
                Append(" DISTINCT ");
            }
        }

        protected virtual void BuildProjections(SelectExpression select)
        {
            if (select.Projections == null || select.Projections.Count == 0)
            {
                Append('*');
            }
            else
            {
                foreach (var item in select.Projections)
                {
                    BuildExpression(item);
                    Append(',');
                }

                SQL.Length--;
            }
        }

        protected virtual void BuildSubQuery(SelectExpression select)
        {
            if (select.SubSelect != null)
            {
                BuildExpression(select.SubSelect);
            }
        }

        protected virtual void BuildFromTable(SelectExpression select)
        {
            if (select.SubSelect == null)
            {
                Utils.CheckNull(select.Table, "query.Table is null");
                Append(select.Table.Schema.Escape(), '.', select.Table.Name.Escape());
            }

            if (!select.Alias.IsNullOrEmpty())
            {
                Append(" AS ", select.Alias.Escape());
            }
        }

        protected virtual void BuildJoins(SelectExpression select)
        {
            if (select.Joins == null || select.Joins.Count == 0)
            {
                return;
            }

            foreach (var join in select.Joins)
            {
                BuildExpression(join);
            }
        }

        protected virtual void BuildSets(SelectExpression select)
        {
            if (select.Sets == null || select.Sets.Count == 0)
            {
                return;
            }

            foreach (var set in select.Sets)
            {
                BuildExpression(set);
            }
        }

        protected virtual void BuildOrders(SelectExpression select)
        {
            if (select.Orders == null || select.Orders.Count == 0)
            {
                return;
            }

            Append(" ORDER BY ");

            foreach (var order in select.Orders)
            {
                BuildExpression(order);
                Append(',');
            }

            SQL.Length--;
        }

        protected virtual void BuildGroups(SelectExpression select)
        {
            if (select.Groups == null || select.Groups.Count == 0)
            {
                return;
            }

            Append(" GROUP BY ");

            foreach (var group in select.Groups)
            {
                BuildExpression(group);
                Append(',');
            }

            SQL.Length--;
        }

        public virtual SQLGenerator Append(params object[] args)
        {
            foreach (var arg in args)
            {
                SQL.Append(arg);
            }

            return this;
        }

        protected virtual void BuildExpression(Expression expression)
        {
            if (expression != null)
            {
                Visit(expression);
            }
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SelectExpression select:
                    VisitQuery(select);
                    break;
                case WhereExperssion where:
                    VisitWhere(where);
                    break;
                case ColumnExpression column:
                    VisitColumn(column);
                    break;
                case JoinExpression join:
                    VisitJoin(join);
                    break;
                case OrderExpression order:
                    VisitOrder(order);
                    break;
                case GroupExpression group:
                    VisitGroup(group);
                    break;
                case UnionExpression union:
                    VisitUnion(union);
                    break;
                case ExceptExpression except:
                    VisitExcept(except);
                    break;
                case IntersectExpression intersect:
                    VisitIntersect(intersect);
                    break;
                case AggregateExpression aggregate:
                    VisitAggregate(aggregate);
                    break;
                case SkipExpression skip:
                    VisitSkip(skip);
                    break;
                case AllExpression all:
                    VisitAll(all);
                    break;
                case AnyExpression any:
                    VisitAny(any);
                    break;
                case ExistsExpression exists:
                    VisitExists(exists);
                    break;
                case DbStoreExpression store:
                    VisitStore(store);
                    break;
                case AliasExpression alias:
                    VisitAlias(alias);
                    break;
            }

            return node;
        }

        protected abstract Expression VisitQuery(SelectExpression select);

        protected abstract Expression VisitSkip(SkipExpression skip);

        protected abstract Expression VisitAll(AllExpression all);

        protected abstract Expression VisitAny(AnyExpression any);

        protected abstract Expression VisitExists(ExistsExpression exists);

        protected abstract Expression VisitStore(DbStoreExpression store);

        protected abstract Expression VisitAggregate(AggregateExpression aggregate);
    }
}
