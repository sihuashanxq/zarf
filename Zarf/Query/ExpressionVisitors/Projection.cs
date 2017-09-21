using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Zarf.Query.ExpressionVisitors
{
    public class Projection
    {
        public Expression Expression { get; set; }

        public int Ordinal { get; set; }

        public MemberInfo Member { get; set; }
    }
}
