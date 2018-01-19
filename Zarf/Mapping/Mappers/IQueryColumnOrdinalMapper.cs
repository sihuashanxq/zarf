using System.Linq.Expressions;
using Zarf.Queries.ExpressionVisitors;

namespace Zarf.Mapping
{
    public interface IQueryColumnOrdinalMapper
    {
        bool IsMapped(Expression node);

        void Map(ColumnDescriptor col);

        int GetOrdinal(Expression node);
    }
}
