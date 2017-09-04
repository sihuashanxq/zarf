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

        public IQueryContext CreateContext()
        {
            //di ProviderFactory
            return new QueryContext(
                    new EntityMemberSourceMappingProvider(),
                    new EntityProjectionMappingProvider(),
                    new PropertyNavigationContext(),
                    new QuerySourceProvider(),
                    new ProjectionExpressionVisitor(),
                    new AliasGenerator()
                );
        }
    }
}
