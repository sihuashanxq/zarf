using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query.Expressions
{
    public class IntersectExpression : SetsExpression
    {
        public IntersectExpression(QueryExpression query)
            : base(query)
        {

        }
    }
}
