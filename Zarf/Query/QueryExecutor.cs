using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Mapping.Bindings;
using Zarf.Queries.Expressions;
using Zarf.Queries.ExpressionTranslators;
using Zarf.Queries.ExpressionVisitors;
using System.Linq;
using System.Collections;
using Zarf.Entities;

namespace Zarf.Queries
{
    public class QueryExecutor : IQueryExecutor
    {
        private IDbContextParts _dbContextParts;

        public QueryExecutor(IDbContextParts dbContextParts)
        {
            _dbContextParts = dbContextParts;
        }

        public IEnumerator<TEntity> Execute<TEntity>(Expression query, IQueryContext queryContext = null)
        {
            var x = typeof(TEntity);
            return ExuecteCore<IEnumerator<TEntity>, TEntity>(query, queryContext);
        }

        public TEntity ExecuteSingle<TEntity>(Expression query, IQueryContext queryContext = null)
        {
            return ExuecteCore<TEntity, TEntity>(query, queryContext);
        }

        public TResult ExuecteCore<TResult, TEntity>(Expression query, IQueryContext queryContext)
        {
            queryContext = queryContext ?? QueryContextFacotry.Factory.CreateContext(dbContextParts: _dbContextParts);

            Expression compiledQuery = null;

            if (query.Is<QueryExpression>())
            {
                compiledQuery = query;
            }
            else
            {
                compiledQuery = new QueryCompiler(queryContext).Compile(query);
            }

            var modelActivator = new DefaultEntityBinder(queryContext).Bind<TEntity>(new BindingContext(compiledQuery));
            var parameters = new List<DbParameter>();

            var commandText = _dbContextParts.CommandTextBuilder.Generate(compiledQuery, parameters);
            var dataReader = _dbContextParts
                .EntityCommandFacotry
                .Create(_dbContextParts.ConnectionString)
                .ExecuteDataReader(commandText, parameters.ToArray());

            if (typeof(TResult).IsCollection())
            {
                return new EntityEnumerator<TEntity>(modelActivator, dataReader).Cast<TResult>();
            }

            using (dataReader)
            {
                if (dataReader.Read())
                {
                    return modelActivator.DynamicInvoke(dataReader).Cast<TResult>();
                }
            }

            return default(TResult);
        }
    }
}