using Zarf.Core;
using Zarf.Mapping;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query
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
                    new QueryColumnOrdinalMapper(),
                    new PropertyNavigationContext(),
                    new QueryMapper(),
                    new AliasGenerator(),
                    new SubQueryValueCache(),
                    dbContextParts
                );
        }

        public IQueryContext CreateContext(
            IQueryColumnOrdinalMapper mappingProvider = null,
            IPropertyNavigationContext navigationContext = null,
            IQueryMapper sourceProvider = null,
            IAliasGenerator aliasGenerator = null,
            ISubQueryValueCache memValue = null,
            IDbContextParts dbcontextParts = null
            )
        {
            return new QueryContext(
                mappingProvider ?? new QueryColumnOrdinalMapper(),
                navigationContext ?? new PropertyNavigationContext(),
                sourceProvider ?? new QueryMapper(),
                aliasGenerator ?? new AliasGenerator(),
                memValue ?? new SubQueryValueCache(),
                dbcontextParts
            );
        }
    }
}
