using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Generators;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Update.Expressions;

namespace Zarf.Sqlite.Generators
{
    /// <summary>
    /// 待改
    /// </summary>
    internal partial class SqliteGenerator : SQLGenerator
    {
        protected override Expression VisitAggregate(AggregateExpression exp)
        {
            switch (exp.Method.Name)
            {
                case "Min":
                    Append("MIN", '(');
                    break;
                case "Max":
                    Append("Max", '(');
                    break;
                case "Sum":
                    Append("Sum", '(');
                    break;
                case "Average":
                    Append("AVG", '(');
                    break;
                case "Count":
                case "LongCount":
                    Append("Count", "(");
                    break;
                default:
                    throw new NotImplementedException($"method {exp.Method.Name} is not supported!");
            }

            if (exp.KeySelector == null)
            {
                Append("1");
            }
            else
            {
                BuildExpression(exp.KeySelector);
            }

            Append(')');

            if (!exp.Alias.IsNullOrEmpty())
            {
                Append(" AS ", exp.Alias);
            }

            return exp;
        }

        protected override Expression VisitQuery(SelectExpression select)
        {
            if (select.IsInPredicate || select.OuterSelect != null)
            {
                Append(" ( ");
            }

            Append(" SELECT  ");

            BuildDistinct(select);

            BuildProjections(select);

            Append(" FROM ");

            BuildSubQuery(select);
            BuildFromTable(select);
            BuildJoins(select);

            BuildExpression(select.Where);

            BuildGroups(select);

            BuildOrders(select);

            BuildLimit(select);

            BuildExpression(select.Offset);

            BuildSets(select);

            if (select.IsInPredicate || select.OuterSelect != null)
            {
                Append(" ) ");
            }

            return select;
        }

        protected override Expression VisitAll(AllExpression all)
        {
            Append(" IF NOT EXISTS(");
            BuildExpression(all.Select);
            Append(") SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)");
            return all;
        }

        protected override Expression VisitAny(AnyExpression any)
        {
            Append(" IF EXISTS(");
            BuildExpression(any.Select);
            Append(") SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)");
            return any;
        }

        protected override Expression VisitSkip(SkipExpression skip)
        {
            Append(" OFFSET ", skip.Offset, " ");
            return skip;
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
            SQL.Length--;
            Append(") VALUES ");

            var parameters = insert.DbParams.ToList();
            var colCount = insert.Columns.Count();

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
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
            var parameters = update.DbParams.ToList();
            for (var i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                var dbParam = parameters[i];
                Append(col.Escape()).
                Append('=').
                Append(dbParam.Name).
                Append(',');
            }

            SQL.Length--;

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

                SQL.Length--;
                Append(')');
            }

            Append(";");
        }

        protected override Expression VisitExists(ExistsExpression exists)
        {
            Append(" EXISTS (");
            BuildExpression(exists.Select);
            Append(") ");
            return exists;
        }
    }
}
