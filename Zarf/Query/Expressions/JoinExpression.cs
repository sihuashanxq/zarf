using System.Linq.Expressions;
using System;
using Zarf.Entities;

namespace Zarf.Query.Expressions
{
    ///<summary>
    ///Represent a join query 
    ///</summary>
    public class JoinExpression : Expression
    {
        public JoinType JoinType { get; }

        public Expression Predicate { get; }

        public FromTableExpression Table { get; }

        public override Type Type => Table?.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public JoinExpression(FromTableExpression table, Expression predicate, JoinType joinType = JoinType.Inner)
        {
            Predicate = predicate;
            Table = table;
            JoinType = joinType;
        }
    }
}