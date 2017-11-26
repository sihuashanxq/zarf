using System.Linq.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Mapping
{
    public interface IEntityProjectionMappingProvider
    {
        bool IsMapped(Expression node);

        void Map(ColumnDescriptor projection);

        int GetOrdinal(Expression node);
    }
}
