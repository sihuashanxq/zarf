using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Zarf.Builders;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Update.Expressions;

namespace Zarf.SqlServer.Builders
{
    internal partial class SqlServerTextBuilder : SqlTextBuilder
    {
        public class DisposeAction : IDisposable
        {
            private Action _action;

            public DisposeAction(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action?.Invoke();
            }
        }

        protected static readonly Dictionary<string, string> Aggregates = new Dictionary<string, string>()
        {
            {"Min","Min" },
            {"Max","Max" },
            {"Sum","Sum" },
            {"Average","Avg" },
            {"Count","Count" },
            {"LongCount","Count_Big" }
        };

        protected bool StopGenColumnAlias { get; set; }

        protected StringBuilder Builder { get; set; } = new StringBuilder();

        protected IDisposable BeginStopGenColumnAlias()
        {
            var stopGenColumnAlias = StopGenColumnAlias;
            StopGenColumnAlias = true;
            return new DisposeAction(() =>
            {
                StopGenColumnAlias = stopGenColumnAlias;
            });
        }

        public override string Build(Expression expression)
        {
            lock (this)
            {
                Builder.Clear();
                BuildExpression(expression);
                return Builder.ToString();
            }
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate)
        {
            if (Aggregates.TryGetValue(aggregate.Method.Name, out string funcName))
            {
                Append(funcName, '(');
                if (aggregate.KeySelector == null || aggregate.Method.Name.Contains("Count"))
                {
                    Append("1");
                }
                else
                {
                    using (BeginStopGenColumnAlias())
                    {
                        BuildExpression(aggregate.KeySelector);
                    }
                }

                Append(')');

                if (!aggregate.Alias.IsNullOrEmpty())
                {
                    Append(" AS ", aggregate.Alias);
                }
            }
            else
            {
                throw new NotImplementedException($"method {aggregate.Method.Name} is not supported!");
            }
            return aggregate;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (column.Query != null && !column.Query.Alias.IsNullOrEmpty())
            {
                Builder.Append(column.Query.Alias.Escape());
                Builder.Append('.');
            }

            if (column.Column == null)
            {
                Append(" NULL ");
            }
            else
            {
                Builder.Append(column.Column.Name.Escape());
            }

            if (!StopGenColumnAlias && !column.Alias.IsNullOrEmpty())
            {
                Append(" AS ");
                Builder.Append(column.Alias.Escape());
            }

            return column;
        }

        protected override Expression VisitAlias(AliasExpression alias)
        {
            Visit(alias.Expression);
            Append(" AS ", alias.Alias);
            return alias;
        }

        protected override Expression VisitExcept(ExceptExpression except)
        {
            Append(" Except ");
            BuildExpression(except.Query);
            return except;
        }

        protected override Expression VisitGroup(GroupExpression group)
        {
            using (BeginStopGenColumnAlias())
            {
                BuildColumns(group.Columns);
                return group;
            }
        }

        protected override Expression VisitIntersect(IntersectExpression intersec)
        {
            Append(" INTERSECT ");
            BuildExpression(intersec.Query);
            return intersec;
        }

        protected override Expression VisitUnion(UnionExpression union)
        {
            Append(" UNION ALL ");
            BuildExpression(union.Query);
            return union;
        }

        protected override Expression VisitJoin(JoinExpression join)
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

            BuildSubQuery(join.Query);

            if (join.Query.IsEmptyQuery())
            {
                BuildFromTable(join.Query);
            }
            else
            {
                BuildExpression(join.Query);
                Append("  AS " + join.Query.Alias.Escape());
            }

            using (BeginStopGenColumnAlias())
            {
                if (join.JoinType != JoinType.Cross)
                {
                    Append(" ON ");
                    BuildExpression(join.Predicate ?? Utils.ExpressionTrue);
                }
            }

            if (join.Query.IsEmptyQuery())
            {
                BuildJoins(join.Query);
            }

            return join;
        }

        protected override Expression VisitOrder(OrderExpression order)
        {
            using (BeginStopGenColumnAlias())
            {
                var direction = order.OrderType == OrderType.Desc
                    ? " DESC "
                    : " ASC ";

                BuildColumns(order.Columns);
                Builder.Append(direction);
            }
            return order;
        }

        protected override Expression VisitQuery(QueryExpression query)
        {
            if (query.IsPartOfPredicate || query.Outer != null)
            {
                Append(" ( ");
            }

            Append(" SELECT  ");

            BuildDistinct(query);
            BuildLimit(query);
            BuildProjections(query);

            Append(" FROM ");

            BuildSubQuery(query);
            BuildFromTable(query);
            BuildJoins(query);

            BuildWhere(query);

            BuildGroups(query);

            BuildOrders(query);

            BuildSets(query);

            if (query.IsPartOfPredicate || query.Outer != null)
            {
                Append(" ) ");
            }

            return query;
        }

        protected override Expression VisitWhere(WhereExperssion where)
        {
            using (BeginStopGenColumnAlias())
            {
                Append(" WHERE ");
                BuildExpression(where.Predicate);
                return where;
            }
        }

        protected override Expression VisitAll(AllExpression all)
        {
            Append(" IF NOT EXISTS(");
            BuildExpression(all.Query);
            Append(") SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)");
            return all;
        }

        protected override Expression VisitAny(AnyExpression any)
        {
            Append(" IF EXISTS(");
            BuildExpression(any.Query);
            Append(") SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)");
            return any;
        }

        protected override Expression VisitSkip(SkipExpression skip)
        {
            Append(" ROW_NUMBER() OVER ( ");
            if (skip.Orders == null || skip.Orders.Count == 0)
            {
                Append("ORDER BY GETDATE()) AS __ROWINDEX__");
            }
            else
            {
                foreach (var order in skip.Orders)
                {
                    BuildExpression(order);
                    Append(',');
                }

                Builder.Length--;
                Append(")  AS __ROWINDEX__");
            }

            return skip;
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            if (constant.Type == typeof(bool))
            {
                Builder.Append(constant.Value.Cast<bool>() ? 1 : 0);
            }
            else if (NumbericTypes.Contains(constant.Type))
            {
                Append(constant.Value);
            }
            else if (constant.Value.Is<DateTime>())
            {
                //998毫秒Sql Server 999毫秒报错
                var date = constant.Value.Cast<DateTime>();
                Append(
                    '\'',
                    date.Year,
                    date.ToString("-MM-dd HH:mm:ss."),
                    date.Millisecond > 998 ? 998 : date.Millisecond,
                    '\'');
            }
            else
            {
                Append('\'', constant.Value.ToString(), '\'');
            }

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
            var leftIsNull = binary.Left.IsNullValueConstant();
            var righIsNull = binary.Right.IsNullValueConstant();
            var left = binary.Left;
            var right = binary.Right;

            if (Utils.OperatorMap.TryGetValue(binary.NodeType, out string op))
            {
                if (leftIsNull && righIsNull)
                {
                    left = right = Expression.Constant(1);
                }
                else if (leftIsNull || righIsNull)
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
                Builder.Append(op);
                BuildExpression(right);

                return binary;
            }

            throw new NotImplementedException("ViistBinary");
        }

        protected virtual void BuildColumns(IEnumerable<ColumnExpression> columns)
        {
            foreach (var column in columns)
            {
                VisitColumn(column);
                Append(',');
            }

            Builder.Length--;
        }

        protected virtual void BuildLimit(QueryExpression query)
        {
            if (query.Limit != 0)
            {
                Append(" TOP  ", query.Limit, " ");
                return;
            }

            if (query.Outer != null && (query.Orders.Count != 0 || query.Groups.Count != 0))
            {
                Append(" TOP (100) Percent ");
            }
        }

        protected virtual void BuildDistinct(QueryExpression query)
        {
            if (query.IsDistinct)
            {
                Append(" DISTINCT ");
            }
        }

        protected virtual void BuildProjections(QueryExpression query)
        {
            if (query.Projections == null || query.Projections.Count == 0)
            {
                Append('*');
            }
            else
            {
                foreach (var item in query.Projections)
                {
                    if (item.Is<AggregateExpression>())
                    {
                        Console.WriteLine("Aggreate");
                        using (BeginStopGenColumnAlias())
                        {

                            var aggrate = item.As<AggregateExpression>();
                            if (aggrate.Query != query)
                            {
                                Append(aggrate.Query.Alias.Escape(), ".", aggrate.Alias.Escape(), ",");
                                continue;
                            }
                        }
                    }

                    BuildExpression(item);
                    Append(',');
                }

                Builder.Length--;
            }
        }

        protected virtual void BuildSubQuery(QueryExpression query)
        {
            if (query.SubQuery != null)
            {
                BuildExpression(query.SubQuery);
            }
        }

        protected virtual void BuildFromTable(QueryExpression query)
        {
            if (query.SubQuery == null)
            {
                Utils.CheckNull(query.Table, "query.Table is null");
                Append(query.Table.Schema.Escape(), '.', query.Table.Name.Escape());
            }

            if (!query.Alias.IsNullOrEmpty())
            {
                Append(" AS ", query.Alias.Escape());
            }
        }

        protected virtual void BuildJoins(QueryExpression query)
        {
            if (query.Joins == null || query.Joins.Count == 0)
            {
                return;
            }

            foreach (var join in query.Joins)
            {
                BuildExpression(join);
            }
        }

        protected virtual void BuildSets(QueryExpression query)
        {
            if (query.Sets == null || query.Sets.Count == 0)
            {
                return;
            }

            foreach (var set in query.Sets)
            {
                BuildExpression(set);
            }
        }

        protected virtual void BuildOrders(QueryExpression query)
        {
            if (query.Orders == null || query.Orders.Count == 0)
            {
                return;
            }

            Append(" ORDER BY ");

            foreach (var order in query.Orders)
            {
                BuildExpression(order);
                Append(',');
            }

            Builder.Length--;
        }

        protected virtual void BuildGroups(QueryExpression query)
        {
            if (query.Groups == null || query.Groups.Count == 0)
            {
                return;
            }

            Append(" GROUP BY ");

            foreach (var group in query.Groups)
            {
                BuildExpression(group);
                Append(',');
            }

            Builder.Length--;
        }

        protected virtual void BuildWhere(QueryExpression query)
        {
            if (query.Where != null)
            {
                VisitWhere(query.Where);
            }
        }

        protected virtual SqlServerTextBuilder Append(params object[] args)
        {
            foreach (var arg in args)
            {
                Builder.Append(arg);
            }

            return this;
        }

        protected virtual void BuildExpression(Expression expression)
        {
            Visit(expression);
        }

        protected override Expression VisitStore(DbStoreExpression store)
        {
            Append("DECLARE @__ROWCOUNT__ INT=0;");
            foreach (var persist in store.Persists)
            {
                switch (persist)
                {
                    case InsertExpression insert:
                        BuildInsert(insert);
                        break;
                    case UpdateExpression update:
                        BuildUpdate(update);
                        break;
                    default:
                        BuildDelete(persist.As<DeleteExpression>());
                        break;
                }

                Append(";SELECT @__ROWCOUNT__=@__ROWCOUNT__+ROWCOUNT_BIG();");
            }

            if (store.Persists.Count == 1 && (
                store.Persists.First().As<InsertExpression>()?.GenerateIdentity ?? false))
            {
                Append("SELECT @__ROWCOUNT__ AS ROWSCOUNT,SCOPE_IDENTITY() AS ID;");
            }
            else
            {
                Append("SELECT @__ROWCOUNT__ AS ROWSCOUNT;");
            }

            return store;
        }

        protected void BuildInsert(InsertExpression insert)
        {
            Append(Environment.NewLine).
            Append(";INSERT INTO ").
            Append(insert.Table.Schema.Escape()).
            Append('.').
            Append(insert.Table.Name.Escape()).
            Append("(");

            foreach (var col in insert.Columns)
            {
                Append(col.Escape()).Append(',');
            }
            Builder.Length--;
            Append(") VALUES ");

            var dbParams = insert.DbParams.ToList();
            var colCount = insert.Columns.Count();

            for (var i = 0; i < dbParams.Count; i++)
            {
                var parameter = dbParams[i];
                var mod = (i % colCount);
                if (mod == 0)
                {
                    Append(i != 0 ? ',' : ' ').
                    Append('(').
                    Append(parameter.Name);
                }
                else
                {
                    Append(',').Append(parameter.Name);
                }

                if ((i + 1) % colCount == 0)
                {
                    Append(')');
                }
            }
        }

        protected void BuildUpdate(UpdateExpression update)
        {
            Append(Environment.NewLine).
            Append(";UPDATE ").
            Append(update.Table.Schema.Escape()).
            Append('.').
            Append(update.Table.Name.Escape()).
            Append("SET ");

            var columns = update.Columns.ToList();
            var dbParams = update.DbParams.ToList();
            for (var i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                var dbParam = dbParams[i];
                Append(col.Escape()).
                Append('=').
                Append(dbParam.Name).
                Append(',');
            }

            Builder.Length--;

            Append(" WHERE ").
            Append(update.Identity).
            Append('=').
            Append(update.IdentityValue.Name).
            Append(";");
        }

        protected void BuildDelete(DeleteExpression delete)
        {
            Append(Environment.NewLine).
            Append(";DELETE FROM  ").
            Append(delete.Table.Schema.Escape()).
            Append('.').
            Append(delete.Table.Name.Escape()).
            Append(" WHERE ").
            Append(delete.PrimaryKey);

            var primaryKeyValues = delete.PrimaryKeyValues.ToList();
            if (primaryKeyValues.Count == 1)
            {
                Append('=');
                Append(delete.PrimaryKeyValues.FirstOrDefault().Name);
            }
            else
            {
                Append("IN (");
                foreach (var primaryKeyValue in primaryKeyValues)
                {
                    Append(primaryKeyValue.Name + ',');
                }

                Builder.Length--;
                Append(')');
            }

            Append(";");
        }

        protected override Expression VisitExists(ExistsExpression exists)
        {
            //if (exists.Query.Columns.Count == 0)
            //{
            //    exists.Query.Columns.Add(new Mapping.ColumnDescriptor() { Expression = Expression.Constant(1) });
            //}

            Append(" EXISTS (");
            BuildExpression(exists.Query);
            Append(") ");
            return exists;
        }
    }
}
