using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Queries.Expressions
{
    public class IntersectExpression : SetsExpression
    {
        public IntersectExpression(QueryExpression query)
            : base(query)
        {

        }
    }
}
