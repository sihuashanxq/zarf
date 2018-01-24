using System.Linq.Expressions;
using Zarf.Query.Expressions;
using Zarf.Query.Mappers;

namespace Zarf.Query.Internals
{
    public class QueryContextFacotry : IQueryContextFactory
    {
        public IQueryContext CreateContext(DbContext dbContext)
        {
            return new QueryContext(
                    new Mapper<Expression, SelectExpression>(),
                    new Mapper<Expression, QueryEntityModel>(),
                    new Mapper<MemberExpression, Expression>(),
                    new Mapper<Expression, Expression>(),
                    new AliasGenerator(),
                    new QueryValueCache(),
                    dbContext
                );
        }
    }
}
