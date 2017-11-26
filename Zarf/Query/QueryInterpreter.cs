using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Mapping.Bindings;
using Zarf.Query.ExpressionTranslators;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query
{
    public class QueryInterpreter : IQueryInterpreter
    {
        private IDbContextParts _dbContextParts;

        public QueryInterpreter(IDbContextParts dbContextParts)
        {
            _dbContextParts = dbContextParts;
        }

        public IEnumerator<TEntity> Execute<TEntity>(Expression query, IQueryContext queryContext = null)
        {
            return ExuecteCore<IEnumerator<TEntity>, TEntity>(query, queryContext);
        }

        public TEntity ExecuteSingle<TEntity>(Expression query, IQueryContext queryContext = null)
        {
            return ExuecteCore<TEntity, TEntity>(query, queryContext);
        }

        public TResult ExuecteCore<TResult, TEntity>(Expression query, IQueryContext queryContext)
        {
            queryContext = queryContext ?? QueryContextFacotry.Factory.CreateContext(dbContextParts: _dbContextParts);

            var compiledQuery = new QueryCompiler(queryContext).Compile(query);
            var entityActivator = new DefaultEntityBinder(queryContext).Bind<TEntity>(new BindingContext(compiledQuery));
            var dataReader = _dbContextParts
                .EntityCommandFacotry
                .Create(_dbContextParts.ConnectionString)
                .ExecuteDataReader(_dbContextParts.CommandTextBuilder.Build(compiledQuery));

            if (typeof(TResult) != typeof(TEntity))
            {
                return new EntityEnumerator<TEntity>(entityActivator, dataReader).Cast<TResult>();
            }

            using (dataReader)
            {
                if (dataReader.Read())
                {
                    return entityActivator(dataReader).Cast<TResult>();
                }
            }

            return default(TResult);
        }
    }
}