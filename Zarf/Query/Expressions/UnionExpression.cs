using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query.Expressions
{
    public class UnionExpression : SetsExpression
    {
        public UnionExpression(QueryExpression query) 
            : base(query)
        {

        }
    }
}
