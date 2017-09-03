using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Query
{
    public interface IProjectionFinder
    {
        List<Expression> FindProjections(Expression expression);
    }
}
