using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Bindings;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Generators;
using Zarf.Metadata.Entities;
using Zarf.Query.Expressions;
using Zarf.Query.Visitors;

namespace Zarf.Query.Internals
{
    public class QueryExecutor : IQueryExecutor
    {
        public ISQLGenerator SQLGenerator { get; }

        public IDbEntityCommandFacotry DbCommandFactory { get; }

        public IDbEntityConnectionFacotry DbConnectionFactory { get; }

        public QueryExecutor(
            ISQLGenerator sqlGenerator,
            IDbEntityCommandFacotry commandFactory,
            IDbEntityConnectionFacotry connectionFacotry)
        {
            DbCommandFactory = commandFactory;
            DbConnectionFactory = connectionFacotry;
            SQLGenerator = sqlGenerator;
        }

        public IEnumerator<TEntity> Execute<TEntity>(Expression query, IQueryContext context)
        {
            return ExuecteCore<IEnumerator<TEntity>, TEntity>(query, context);
        }

        public TEntity ExecuteSingle<TEntity>(Expression query, IQueryContext context)
        {
            return ExuecteCore<TEntity, TEntity>(query, context);
        }

        public TResult ExuecteCore<TResult, TEntity>(Expression expression, IQueryContext queryContext)
        {
            var compiledQuery = expression.NodeType == ExpressionType.Extension
                ? expression
                : new QueryExpressionVisitor(queryContext).Compile(expression);

            var binder = new ModelBinder(queryContext);
            var modelActivator = binder.Bind<TEntity>(new BindingContext(compiledQuery, this));

            var parameters = new List<DbParameter>();
            var commandText = SQLGenerator.Generate(compiledQuery, parameters);
            var command = DbCommandFactory.Create(DbConnectionFactory.Create());
            var reader = command.ExecuteDataReader(commandText, parameters.ToArray());

            if (typeof(TResult).IsCollection())
            {
                return new EntityEnumerator<TEntity>(modelActivator, reader).Cast<TResult>();
            }

            using (reader)
            {
                if (reader.Read())
                {
                    try
                    {
                        return modelActivator.DynamicInvoke(reader).Cast<TResult>();
                    }
                    finally
                    {
                        var limit = compiledQuery.As<SelectExpression>()?.Limit ?? 1;
                        if (limit > 1 && reader.Read())
                        {
                            throw new Exception("Sequence contains more than one matching element!");
                        }
                    }
                }
            }

            var defaultIfEmpty = compiledQuery.As<SelectExpression>()?.DefaultIfEmpty ?? false;
            if (!defaultIfEmpty)
            {
                throw new Exception("Sequence contains no matching element");
            }

            return default(TResult);
        }
    }
}