using System.Linq.Expressions;
using System.Collections.Generic;
using System;

namespace Zarf.Update.Expressions
{
    public class DbStoreExpression : Expression
    {
        /// <summary>
        /// InsertExpression UpdateExpression DeleteExpression
        /// </summary>
        public IEnumerable<Expression> Persists { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(void);

        public DbStoreExpression(IEnumerable<Expression> persists)
        {
            Persists = persists;
        }
    }
}
