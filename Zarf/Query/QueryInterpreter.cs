using System.Linq.Expressions;
using Zarf.Query.ExpressionVisitors;
using Zarf.Query.ExpressionTranslators;
using Zarf.Mapping.Bindings;
using Zarf.Builders;
using System.Collections.Generic;
using System.Reflection;
using Zarf.Extensions;

namespace Zarf.Query
{
    public class QueryInterpreter : IQueryInterpreter
    {
        public IEnumerator<TEntity> Execute<TEntity>(Expression query, IQueryContext queryContext = null)
        {
            return ExuecteCore<IEnumerator<TEntity>, TEntity>(query, queryContext);
        }

        public TEntity ExecuteSingle<TEntity>(Expression query, IQueryContext queryContext = null)
        {
            return ExuecteCore<TEntity, TEntity>(query, queryContext);
        }

        public TResult ExuecteCore<TResult, TElement>(Expression query, IQueryContext queryContext)
        {
            if (queryContext == null)
            {
                queryContext = QueryContextFacotry.Factory.CreateContext();
            }

            var compiler = new QueryCompiler(queryContext, NodeTypeTranslatorProvider.Default);
            var compiledQuery = compiler.Compile(query);
            var entityBinder = new DefaultEntityBinder(queryContext);
            var entityCreator = entityBinder.Bind(new BindingContext(compiledQuery));
            var commandText = new SqlServerTextBuilder().Build(compiledQuery);
            var dataReader = new DbCommand(commandText).ExecuteReader();

            if (typeof(TResult) != typeof(TElement))
            {
                return new EntityEnumerator<TElement>(entityCreator, dataReader).Cast<TResult>();
            }

            if (dataReader.Read())
            {
                return entityCreator.DynamicInvoke(dataReader).Cast<TResult>();
            }

            return default(TResult);
        }
    }
}