using Zarf.Core;
using Zarf.Mapping;
using Zarf.Queries.ExpressionVisitors;

namespace Zarf.Queries
{
    public class QueryContextFacotry : IQueryContextFactory
    {
        public static readonly IQueryContextFactory Factory = new QueryContextFacotry();

        private QueryContextFacotry()
        {
        }

        public IQueryContext CreateContext(IDbContextParts dbContextParts)
        {
            return new QueryContext(
                    new QueryMapper(),
                    new AliasGenerator(),
                    new SubQueryValueCache(),
                    dbContextParts
                );
        }

        public IQueryContext CreateContext(
            IQueryMapper sourceProvider = null,
            IAliasGenerator aliasGenerator = null,
            ISubQueryValueCache memValue = null,
            IDbContextParts dbcontextParts = null
            )
        {
            return new QueryContext(
                sourceProvider ?? new QueryMapper(),
                aliasGenerator ?? new AliasGenerator(),
                memValue ?? new SubQueryValueCache(),
                dbcontextParts
            );
        }
    }
}
