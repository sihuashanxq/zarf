using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query.Expressions
{
    public class ExceptExpression : SetsExpression
    {
        public ExceptExpression(SelectExpression select)
            : base(select)
        {

        }
    }
}
