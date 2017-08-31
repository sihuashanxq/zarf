using System.Linq.Expressions;

namespace Zarf.Mapping
{
    public interface IEntityMemberMappingProvider
    {
        void Map(Expression mem, Expression source, int ordinal);

        void Map(Expression mem, Expression source);

        IMapping GetMapping(Expression mem);
    }
}
