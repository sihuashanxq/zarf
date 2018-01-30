using System;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Generators;
using Zarf.Generators.Functions.Providers;
using Zarf.Query.Expressions;
using Zarf.Update.Expressions;

namespace Zarf.SqlServer.Generators
{
    internal partial class SqlServerGenerator : SQLGenerator
    {
        public SqlServerGenerator(ISQLFunctionHandlerProvider provider)
            : base(provider)
        {

        }

        protected override Expression VisitAggregate(AggregateExpression exp)
        {
            switch (exp.Method.Name)
            {
                case "Min":
                    AttachSQL("MIN", '(');
                    break;
                case "Max":
                    AttachSQL("Max", '(');
                    break;
                case "Sum":
                    AttachSQL("Sum", '(');
                    break;
                case "Average":
                    AttachSQL("AVG", '(');
                    break;
                case "Count":
                    AttachSQL("Count", "(");
                    break;
                case "LongCount":
                    AttachSQL("Count_Big", "(");
                    break;
                default:
                    throw new NotImplementedException($"method {exp.Method.Name} is not supported!");
            }

            if (exp.KeySelector == null)
            {
                AttachSQL("1");
            }
            else
            {
                Attach(exp.KeySelector);
            }
            AttachSQL(')');

            if (!exp.Alias.IsNullOrEmpty())
            {
                AttachSQL(" AS ", exp.Alias);
            }

            return exp;
        }

        protected override Expression VisitQuery(SelectExpression select)
        {
            if (select.IsInPredicate || select.OuterSelect != null)
            {
                AttachSQL(" ( ");
            }

            AttachSQL(" SELECT  ");

            BuildDistinct(select);
            BuildLimit(select);
            BuildProjections(select);

            AttachSQL(" FROM ");

            BuildSubQuery(select);
            BuildFromTable(select);
            BuildJoins(select);

            Attach(select.Where);

            BuildGroups(select);

            BuildOrders(select);

            BuildSets(select);

            if (select.IsInPredicate || select.OuterSelect != null)
            {
                AttachSQL(" ) ");
            }

            return select;
        }

        protected override Expression VisitAll(AllExpression all)
        {
            AttachSQL(" IF NOT EXISTS(");
            Attach(all.Select);
            AttachSQL(") SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)");
            return all;
        }

        protected override Expression VisitAny(AnyExpression any)
        {
            AttachSQL(" IF EXISTS(");
            Attach(any.Select);
            AttachSQL(") SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)");
            return any;
        }

        protected override Expression VisitSkip(SkipExpression skip)
        {
            AttachSQL(" ROW_NUMBER() OVER ( ");
            if (skip.Orders == null || skip.Orders.Count == 0)
            {
                AttachSQL("ORDER BY GETDATE()) AS __ROWINDEX__");
            }
            else
            {
                foreach (var order in skip.Orders)
                {
                    Attach(order);
                    AttachSQL(',');
                }

                SQL.Length--;
                AttachSQL(")  AS __ROWINDEX__");
            }

            return skip;
        }

        protected override void BuildLimit(SelectExpression select)
        {
            if (select.Limit != 0)
            {
                AttachSQL(" TOP  ", select.Limit, " ");
                return;
            }

            if (select.OuterSelect != null && (select.Orders.Count != 0 || select.Groups.Count != 0))
            {
                AttachSQL(" TOP (100) Percent ");
            }
        }

        protected override Expression VisitStore(DbStoreExpression store)
        {
            AttachSQL("DECLARE @__ROWCOUNT__ INT=0;");
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

                AttachSQL(";SELECT @__ROWCOUNT__=@__ROWCOUNT__+ROWCOUNT_BIG();");
            }

            if (store.Persists.Count == 1 && (
                store.Persists.First().As<InsertExpression>()?.GenerateIdentity ?? false))
            {
                AttachSQL("SELECT @__ROWCOUNT__ AS ROWSCOUNT,SCOPE_IDENTITY() AS ID;");
            }
            else
            {
                AttachSQL("SELECT @__ROWCOUNT__ AS ROWSCOUNT;");
            }

            return store;
        }

        protected void BuildInsert(InsertExpression insert)
        {
            AttachSQL(Environment.NewLine).
            AttachSQL(";INSERT INTO ").
            AttachSQL(insert.Table.Schema.Escape()).
            AttachSQL('.').
            AttachSQL(insert.Table.Name.Escape()).
            AttachSQL("(");

            foreach (var col in insert.Columns)
            {
                AttachSQL(col.Escape()).AttachSQL(',');
            }
            SQL.Length--;
            AttachSQL(") VALUES ");

            var parameters = insert.DbParams.ToList();
            var colCount = insert.Columns.Count();

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var mod = (i % colCount);
                if (mod == 0)
                {
                    AttachSQL(i != 0 ? ',' : ' ').
                    AttachSQL('(').
                    AttachSQL(parameter.Name);
                }
                else
                {
                    AttachSQL(',').AttachSQL(parameter.Name);
                }

                if ((i + 1) % colCount == 0)
                {
                    AttachSQL(')');
                }
            }
        }

        protected void BuildUpdate(UpdateExpression update)
        {
            AttachSQL(Environment.NewLine).
            AttachSQL(";UPDATE ").
            AttachSQL(update.Table.Schema.Escape()).
            AttachSQL('.').
            AttachSQL(update.Table.Name.Escape()).
            AttachSQL("SET ");

            var columns = update.Columns.ToList();
            var parameters = update.DbParams.ToList();
            for (var i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                var dbParam = parameters[i];
                AttachSQL(col.Escape()).
                AttachSQL('=').
                AttachSQL(dbParam.Name).
                AttachSQL(',');
            }

            SQL.Length--;

            AttachSQL(" WHERE ").
            AttachSQL(update.Identity).
            AttachSQL('=').
            AttachSQL(update.IdentityValue.Name).
            AttachSQL(";");
        }

        protected void BuildDelete(DeleteExpression delete)
        {
            AttachSQL(Environment.NewLine).
            AttachSQL(";DELETE FROM  ").
            AttachSQL(delete.Table.Schema.Escape()).
            AttachSQL('.').
            AttachSQL(delete.Table.Name.Escape()).
            AttachSQL(" WHERE ").
            AttachSQL(delete.PrimaryKey);

            var primaryKeyValues = delete.PrimaryKeyValues.ToList();
            if (primaryKeyValues.Count == 1)
            {
                AttachSQL('=');
                AttachSQL(delete.PrimaryKeyValues.FirstOrDefault().Name);
            }
            else
            {
                AttachSQL("IN (");
                foreach (var primaryKeyValue in primaryKeyValues)
                {
                    AttachSQL(primaryKeyValue.Name + ',');
                }

                SQL.Length--;
                AttachSQL(')');
            }

            AttachSQL(";");
        }

        protected override Expression VisitExists(ExistsExpression exists)
        {
            AttachSQL(" EXISTS (");
            Attach(exists.Select);
            AttachSQL(") ");
            return exists;
        }
    }
}
