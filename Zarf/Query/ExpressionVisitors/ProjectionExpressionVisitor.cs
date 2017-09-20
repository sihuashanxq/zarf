using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public class Projection
    {
        public Expression Expression { get; set; }

        public int Ordinal { get; set; }
    }

    public class ProjectionExpressionVisitor : ExpressionVisitor, IProjectionScanner
    {
        private List<Projection> _projections;

        protected override Expression VisitExtension(Expression node)
        {
            if (node.Is<ColumnExpression>() || node.Is<AggregateExpression>())
            {
                _projections.Add(new Projection()
                {
                    Expression = node,
                    Ordinal = _projections.Count
                });
            }
            else if (node.Is<FromTableExpression>())
            {
                foreach (var item in node.As<FromTableExpression>().GenerateColumns())
                {
                    _projections.Add(new Projection()
                    {
                        Expression = item,
                        Ordinal = _projections.Count
                    });
                }
            }

            return node;
        }

        public List<Projection> Scan(Expression node)
        {
            _projections = new List<Projection>();
            Visit(node);
            return _projections.ToList();
        }

        public List<Projection> Scan(Func<Expression, Expression> preHandle, Expression node)
        {
            return Scan(preHandle(node));
        }
    }
}
