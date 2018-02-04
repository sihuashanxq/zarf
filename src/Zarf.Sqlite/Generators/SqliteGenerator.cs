using System;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Generators;
using Zarf.Generators.Functions.Providers;
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
            AttachSQL(" OFFSET ", skip.Offset, " ");
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
                AttachSQL(";SELECT 1 ROWSCOUNT,LAST_INSERT_ROWID() AS ID;");
            }
            else
            {
                AttachSQL(";SELECT changes() AS ROWSCOUNT;");
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
            AttachSQL(") ");

            var parameters = insert.DbParams.ToList();
            var colCount = insert.Columns.Count();

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var mod = (i % colCount);
                if (mod == 0)
                {
                    AttachSQL(i != 0 ? " UNION " : " ").
                    AttachSQL(" SELECT ").
                    AttachSQL(parameter.Name);
                }
                else
                {
                    AttachSQL(',').AttachSQL(parameter.Name);
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
