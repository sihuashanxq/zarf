using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query
{
    public interface IQueryContext
    {
        IEntityMemberMappingProvider EntityMemberMappingProvider { get; }

        IPropertyNavigationContext PropertyNavigationContext { get; }

        IQuerySourceProvider QuerySourceProvider { get; }

        IProjectionFinder ProjectionFinder { get; }

        IAliasGenerator AliasGenerator { get; }
    }

    public interface IEntityMemberMappingProvider
    {
        void Map(Expression expression, Expression querySource, int ordinal);
    }
}
