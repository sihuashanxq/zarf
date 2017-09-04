using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public interface IEntityProjectionMappingProvider
    {
        void Map(Expression refrenceProjection, Expression source, int ordinal);

        IEntityProjectionMapping GetMapping(Expression refrenceProjection);
    }
}
