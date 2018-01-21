using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Query.ExpressionVisitors;
using System.Linq;
using Zarf.Generators;
using Zarf.Metadata.Entities;
using Zarf.Bindings;

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

        public TResult ExuecteCore<TResult, TEntity>(Expression query, IQueryContext context)
        {
            var compiledQuery = query.NodeType == ExpressionType.Extension
                ? query
                : new QueryCompiler(context).Compile(query);

            var parameters = new List<DbParameter>();
            var bindingContext = new BindingContext(compiledQuery, this);
            var modelActivator = new ModelBinder(context).Bind<TEntity>(bindingContext);

            var commandText = SQLGenerator.Generate(compiledQuery, parameters);
            var command = DbCommandFactory.Create(DbConnectionFactory.Create());
            var dataReader = command.ExecuteDataReader(commandText, parameters.ToArray());

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