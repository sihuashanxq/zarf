using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Generators;
using Zarf.Generators.Functions.Providers;
using Zarf.Metadata.Entities;
using Zarf.Query.Expressions;
using Zarf.Update.Expressions;

namespace Zarf.Sqlite.Generators
{
    internal partial class SqliteGenerator : SQLGenerator
    {
        public SqliteGenerator(ISQLFunctionHandlerProvider provider)
            : base(provider)
        {

        }

        public override string Generate(Expression expression, List<DbParameter> parameters)
        {
            return base.Generate(expression, parameters).Replace("[dbo].", string.Empty);
        }

        protected override Expression VisitAggregate(AggregateExpression exp)
        {
            if (exp.Method.Name != "LongCount")
            {
                return base.VisitAggregate(exp);
            }

            if (exp.KeySelector == null)
            {
                Attach("1");
            }
            else
            {
                Attach(exp.KeySelector);
            }

            Attach(" ) ");
            Attach(!exp.Alias.IsNullOrEmpty() ? " AS " + exp.Alias : string.Empty);

            return exp;
        }

        protected override Expression VisitQuery(SelectExpression select)
        {
            if (select.IsInPredicate || select.OuterSelect != null)
            {
                Attach(" ( ");
            }

            Attach(" SELECT  ");

            BuildDistinct(select);

            BuildProjections(select);

            Attach(" FROM ");

            BuildSubQuery(select);
            BuildFromTable(select);
            BuildJoins(select);

            Attach(select.Where);

            BuildGroups(select);

            BuildOrders(select);

            BuildLimit(select);

            Attach(select.Offset);

            BuildSets(select);

            if (select.IsInPredicate || select.OuterSelect != null)
            {
                Attach(" ) ");
            }

            return select;
        }

        protected override Expression VisitSkip(SkipExpression skip)
        {
            Attach(" OFFSET ");
            Attach(skip.Offset.ToString());
            Attach(" ");
            return skip;
        }

        protected override Expression VisitStore(DbStoreExpression store)
        {
            switch (store.Persists[0])
            {
                case InsertExpression insert:
                    BuildInsert(insert);
                    break;
                case UpdateExpression update:
                    BuildUpdate(update);
                    break;
                default:
                    BuildDelete(store.Persists[0].As<DeleteExpression>());
                    break;
            }

            if (store.Persists.Count == 1 && (
                store.Persists.First().As<InsertExpression>()?.GenerateIdentity ?? false))
            {
                Attach(";SELECT 1 ROWSCOUNT,LAST_INSERT_ROWID() AS ID;");
            }
            else
            {
                Attach(";SELECT changes() AS ROWSCOUNT;");
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
            Attach(") ");

            var parameters = insert.DbParams.ToList();
            var colCount = insert.Columns.Count();

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var mod = (i % colCount);
                if (mod == 0)
                {
                    Attach(i != 0 ? " UNION " : " ");
                    Attach(" SELECT ");
                    Attach(parameter.Name);
                }
                else
                {
                    Attach(",");
                    Attach(parameter.Name);
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
