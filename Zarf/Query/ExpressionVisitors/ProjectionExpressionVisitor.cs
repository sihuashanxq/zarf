using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Zarf.Query.ExpressionVisitors
{
    public class ProjectionExpressionVisitor : ExpressionVisitor, IProjectionScanner
    {
        private List<Expression> _extensionExpressions;

        protected override Expression VisitExtension(Expression extension)
        {
            _extensionExpressions.Add(extension);
            return extension;
        }

        public List<TRefrence> Scan<TRefrence>(Expression node)
            where TRefrence : Expression
        {
            _extensionExpressions = new List<Expression>();
            Visit(node);
            return _extensionExpressions.OfType<TRefrence>().ToList();
        }

        public List<TRefrence> Scan<TRefrence>(Func<Expression, Expression> preHandle, Expression node)
            where TRefrence : Expression
        {
            return Scan<TRefrence>(preHandle(node));
        }

        public List<Expression> Scan(Expression node)
        {
            return Scan<Expression>(node);
        }

        public List<Expression> Scan(Func<Expression, Expression> preHandle, Expression node)
        {
            return Scan<Expression>(preHandle, node);
        }
    }
}
