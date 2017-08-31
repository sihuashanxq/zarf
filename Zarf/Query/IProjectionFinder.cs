using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Zarf.Query
{

    public interface IProjectionFinder
    {
        List<Expression> FindProjections(Expression expression);
    }
}
