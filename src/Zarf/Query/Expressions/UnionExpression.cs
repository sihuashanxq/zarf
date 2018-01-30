using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query.Expressions
{
    public class UnionExpression : SetsExpression
    {
        public UnionExpression(SelectExpression select) 
            : base(select)
        {

        }
    }
}
