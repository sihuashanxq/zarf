using System;
using System.Linq.Expressions;
using Zarf.Entities;

namespace Zarf.Query.Statements
{
    public interface IQuerySource
    {

    }

    public class Statement
    {
        IQuerySource QuerySource { get; }

        public Statement Prev { get; }

        public Statement Next { get; }
    }

    public class FromStatement : Statement
    {
        public Table Table { get; }
    }

    public class WhereStatement : Statement
    {
        public LambdaExpression Precidate { get; }
    }
}