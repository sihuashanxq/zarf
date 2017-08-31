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

        string NewAlias();
    }

    public interface IEntityMemberMappingProvider
    {
        void Map(Expression expression, Expression querySource, int ordinal);
    }
}
