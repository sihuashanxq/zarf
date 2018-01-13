using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Mapping.Bindings;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators;
using Zarf.Query.ExpressionVisitors;
using System.Linq;
using System.Collections;

namespace Zarf.Query
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

            var entityActivator = new DefaultEntityBinder(queryContext).Bind<TEntity>(new BindingContext(compiledQuery));
            var dataReader = _dbContextParts
                .EntityCommandFacotry
                .Create(_dbContextParts.ConnectionString)
                .ExecuteDataReader(_dbContextParts.CommandTextBuilder.Build(compiledQuery));

            if (typeof(TResult).IsCollection())
            {
                return new EntityEnumerator<TEntity>(entityActivator, dataReader).Cast<TResult>();
            }

            using (dataReader)
            {
                if (dataReader.Read())
                {
                    return entityActivator.DynamicInvoke(dataReader).Cast<TResult>();
                }
            }

            return default(TResult);
        }
    }
}