using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.Mapping.Bindings;
using Zarf.Queries.Expressions;
using Zarf.Queries.ExpressionVisitors;
using System.Linq;
using Zarf.Entities;
using Zarf.Generators;

namespace Zarf.Queries.Internals
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

            var bindingContext = new BindingContext(compiledQuery, this);
            var modelActivator = new DefaultEntityBinder(context).Bind<TEntity>(bindingContext);
            var parameters = new List<DbParameter>();
            var commandText = SQLGenerator.Generate(compiledQuery, parameters);
            var dbConnection = DbConnectionFactory.Create();
            var dbCommand = DbCommandFactory.Create(dbConnection);
            var dataReader = dbCommand.ExecuteDataReader(commandText, parameters.ToArray());

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