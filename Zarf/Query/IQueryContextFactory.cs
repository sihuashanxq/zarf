using System;
using System.Collections.Generic;
using System.Text;
using Zarf.Mapping;
namespace Zarf.Query
{
    public interface IQueryContextFactory
    {
        IQueryContext CreateContext();

        IQueryContext CreateContext(
            IEntityMemberSourceMappingProvider sourceMappingProvider = null,
            IEntityProjectionMappingProvider mappingProvider = null,
            IPropertyNavigationContext navigationContext = null,
            IQuerySourceProvider sourceProvider = null,
            IProjectionScanner scanner = null,
            IAliasGenerator generator = null,
            IMemberValueCache memValue = null);
    }
}
