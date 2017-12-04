using Zarf.Mapping;
using System.Collections.Generic;
using Zarf.Query.Expressions;
using Zarf.Core;
using System.Linq.Expressions;
using Zarf.Query.Internals;

namespace Zarf.Query
{
    public interface IQueryContext
    {
        IMemberAccessMapper MemberAccessMapper { get; }

        ILambdaParameterMapper LambdaParameterMapper { get; }

        IProjectionScanner ProjectionScanner { get; }

        IPropertyNavigationContext PropertyNavigationContext { get; }

        IEntityProjectionMappingProvider ProjectionMappingProvider { get; }

        IAliasGenerator Alias { get; }

        IMemberValueCache MemberValueCache { get; }

        IDbContextParts DbContextParts { get; }

        IQueryColumnCaching ColumnCaching { get; }
    }
}
