using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
namespace Zarf.Queries.Expressions
{
    public class SqlFunctionExpression : Expression
    {
        public MethodInfo Method { get; }

        public Expression Object { get; }

        public string Name { get; }

        public List<Expression> Arguments { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Method.ReturnType;

        public SqlFunctionExpression(MethodInfo methodInfo, string name, Expression @object, List<Expression> arguments)
        {
            Method = methodInfo;
            Arguments = arguments;
            Object = @object;
            Name = name;
        }
    }
}
