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
                    new EntityMemberSourceMappingProvider(),
                    new EntityProjectionMappingProvider(),
                    new PropertyNavigationContext(),
                    new QuerySourceProvider(),
                    new ProjectionExpressionVisitor(),
                    new AliasGenerator(),
                    new MemberValueCache(),
                    dbContextParts
                );
        }

        public IQueryContext CreateContext(
            IEntityMemberSourceMappingProvider sourceMappingProvider = null,
            IEntityProjectionMappingProvider mappingProvider = null,
            IPropertyNavigationContext navigationContext = null,
            IQuerySourceProvider sourceProvider = null,
            IProjectionScanner scanner = null,
            IAliasGenerator aliasGenerator = null,
            IMemberValueCache memValue = null,
            IDbContextParts dbcontextParts = null
            )
        {
            return new QueryContext(
                sourceMappingProvider ?? new EntityMemberSourceMappingProvider(),
                mappingProvider ?? new EntityProjectionMappingProvider(),
                navigationContext ?? new PropertyNavigationContext(),
                sourceProvider ?? new QuerySourceProvider(),
                scanner ?? new ProjectionExpressionVisitor(),
                aliasGenerator ?? new AliasGenerator(),
                memValue ?? new MemberValueCache(),
                dbcontextParts
            );
        }
    }
}
