using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Zarf.Query
{
    public class ExpressionVisitorProjectionFinder : ExpressionVisitor, IProjectionFinder
    {
        private List<Expression> _extensionExpressions;

        protected override Expression VisitExtension(Expression extension)
        {
            _extensionExpressions.Add(extension);
            return extension;
        }

        public List<Expression> FindProjections(Expression expression)
        {
            _extensionExpressions = new List<Expression>();
            Visit(expression);
            return _extensionExpressions;
        }
    }
}
