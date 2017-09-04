using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Zarf.Query.ExpressionVisitors
{
    public class ProjectionExpressionVisitor : ExpressionVisitor, IRefrenceProjectionFinder
    {
        private List<Expression> _extensionExpressions;

        protected override Expression VisitExtension(Expression extension)
        {
            _extensionExpressions.Add(extension);
            return extension;
        }

        public List<TRefrence> Find<TRefrence>(Expression node)
            where TRefrence : Expression
        {
            _extensionExpressions = new List<Expression>();
            Visit(node);
            return _extensionExpressions.OfType<TRefrence>().ToList();
        }

        public List<TRefrence> Find<TRefrence>(Func<Expression, Expression> preHandle, Expression node)
            where TRefrence : Expression
        {
            return Find<TRefrence>(preHandle(node));
        }

        public List<Expression> Find(Expression node)
        {
            return Find<Expression>(node);
        }

        public List<Expression> Find(Func<Expression, Expression> preHandle, Expression node)
        {
            return Find<Expression>(preHandle, node);
        }
    }
}
