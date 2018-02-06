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

        protected override Expression VisitQuery(SelectExpression select)
        {
            if (select.IsInPredicate || select.OuterSelect != null)
            {
                Attach(" ( ");
            }

            Attach(" SELECT  ");

            BuildDistinct(select);
            BuildLimit(select);
            BuildProjections(select);

            Attach(" FROM ");

            BuildSubQuery(select);
            BuildFromTable(select);
            BuildJoins(select);

            Attach(select.Where);

            BuildGroups(select);

            BuildOrders(select);

            BuildSets(select);

            if (select.IsInPredicate || select.OuterSelect != null)
            {
                Attach(" ) ");
            }

            return select;
        }

        protected override Expression VisitSkip(SkipExpression skip)
        {
            Attach(" ROW_NUMBER() OVER ( ");
            if (skip.Orders == null || skip.Orders.Count == 0)
            {
                Attach("ORDER BY GETDATE()) AS __ROWINDEX__");
            }
            else
            {
                foreach (var order in skip.Orders)
                {
                    Attach(order);
                    Attach(",");
                }

                SQL.Length--;
                Attach(")  AS __ROWINDEX__");
            }

            return skip;
        }

        protected override void BuildLimit(SelectExpression select)
        {
            if (select.Limit != 0)
            {
                Attach(" TOP  ");
                Attach(select.Limit.ToString());
                Attach(" ");
                return;
            }

            if (select.OuterSelect != null && (select.Orders.Count != 0 || select.Groups.Count != 0))
            {
                Attach(" TOP (100) Percent ");
            }
        }

        protected override Expression VisitStore(DbStoreExpression store)
        {
            Attach("DECLARE @__ROWCOUNT__ INT=0;");
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

                Attach(";SELECT @__ROWCOUNT__=@__ROWCOUNT__+ROWCOUNT_BIG();");
            }

            if (store.Persists.Count == 1 && (
                store.Persists.First().As<InsertExpression>()?.GenerateIdentity ?? false))
            {
                Attach("SELECT @__ROWCOUNT__ AS ROWSCOUNT,SCOPE_IDENTITY() AS ID;");
            }
            else
            {
                Attach("SELECT @__ROWCOUNT__ AS ROWSCOUNT;");
            }

            return store;
        }

        protected void BuildInsert(InsertExpression insert)
        {
            Attach(Environment.NewLine);
            Attach(";INSERT INTO ");
            Attach(insert.Table.Schema.Escape());
            Attach(".");
            Attach(insert.Table.Name.Escape());
            Attach("(");

            foreach (var col in insert.Columns)
            {
                Attach(col.Escape());
                Attach(",");
            }
            SQL.Length--;
            Attach(") VALUES ");

            var parameters = insert.DbParams.ToList();
            var colCount = insert.Columns.Count();

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var mod = (i % colCount);
                if (mod == 0)
                {
                    Attach(i != 0 ? "," : " ");
                    Attach("(");
                    Attach(parameter.Name);
                }
                else
                {
                    Attach(",");
                    Attach(parameter.Name);
                }

                if ((i + 1) % colCount == 0)
                {
                    Attach(")");
                }
            }
        }

        protected void BuildUpdate(UpdateExpression update)
        {
            Attach(Environment.NewLine);
            Attach(";UPDATE ");
            Attach(update.Table.Schema.Escape());
            Attach(".");
            Attach(update.Table.Name.Escape());
            Attach("SET ");

            var columns = update.Columns.ToList();
            var parameters = update.DbParams.ToList();
            for (var i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                var dbParam = parameters[i];
                Attach(col.Escape());
                Attach("=");
                Attach(dbParam.Name);
                Attach(",");
            }

            SQL.Length--;

            Attach(" WHERE ");
            Attach(update.Identity);
            Attach("=");
            Attach(update.IdentityValue.Name);
            Attach(";");
        }

        protected void BuildDelete(DeleteExpression delete)
        {
            Attach(Environment.NewLine);
            Attach(";DELETE FROM  ");
            Attach(delete.Table.Schema.Escape());
            Attach(".");
            Attach(delete.Table.Name.Escape());
            Attach(" WHERE ");
            Attach(delete.PrimaryKey);

            var primaryKeyValues = delete.PrimaryKeyValues.ToList();
            if (primaryKeyValues.Count == 1)
            {
                Attach("=");
                Attach(delete.PrimaryKeyValues.FirstOrDefault().Name);
            }
            else
            {
                Attach("IN (");
                foreach (var primaryKeyValue in primaryKeyValues)
                {
                    Attach(primaryKeyValue.Name + ",");
                }

                SQL.Length--;
                Attach(")");
            }

            Attach(";");
        }

        protected override Expression VisitExists(ExistsExpression exists)
        {
            Attach(" EXISTS (");
            Attach(exists.Select);
            Attach(") ");
            return exists;
        }
    }
}
