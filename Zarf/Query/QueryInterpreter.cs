using System.Linq.Expressions;
using Zarf.Query.ExpressionVisitors;
using Zarf.Query.ExpressionTranslators;
using Zarf.Mapping.Bindings;
using System.Collections.Generic;
using Zarf.Extensions;
using System;
using Microsoft.Extensions.DependencyInjection;
using Zarf.Builders;
using Zarf.Core;

namespace Zarf.Query
{
    public class QueryInterpreter : IQueryInterpreter
    {
        private IServiceProvider _serviceProvider;

        private IDbCommandFacade _dbCommand;

        public QueryInterpreter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dbCommand = serviceProvider.GetService<IDbCommandFacade>();
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
            if (queryContext == null)
            {
                queryContext = QueryContextFacotry.Factory.CreateContext();
            }

            var compiler = new QueryCompiler(queryContext, NodeTypeTranslatorProvider.Default);
            var compiledQuery = compiler.Compile(query);
            var entityBinder = new DefaultEntityBinder(queryContext);
            var entityCreator = entityBinder.Bind<TEntity>(new BindingContext(compiledQuery));

            var commandText = _serviceProvider.GetService<ISqlTextBuilder>().Build(compiledQuery);
            var dataReader = _dbCommand.ExecuteReader(commandText);

            if (typeof(TResult) != typeof(TEntity))
            {
                return new EntityEnumerator<TEntity>(entityCreator, dataReader).Cast<TResult>();
            }

            if (dataReader.Read())
            {
                return entityCreator(dataReader).Cast<TResult>();
            }

            return default(TResult);
        }
    }
}