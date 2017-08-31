using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query.Expressions
{
    public class ExceptExpression : SetsExpression
    {
        public ExceptExpression(QueryExpression query)
            : base(query)
        {

        }
    }
}
