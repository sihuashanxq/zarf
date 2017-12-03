using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Mapping;

namespace Zarf.Query.ExpressionVisitors
{
    public class ProjectionExpressionVisitor : ExpressionVisitor, IProjectionScanner
    {
        private List<ColumnDescriptor> _columns;

        public List<ColumnDescriptor> Scan(Expression node)
        {
            lock (this)
            {
                _columns = new List<ColumnDescriptor>();
                Visit(node);
                return _columns.ToList();
            }
        }

        public List<ColumnDescriptor> Scan(Func<Expression, Expression> preHandle, Expression node)
        {
            return Scan(preHandle(node));
        }

        protected override Expression VisitExtension(Expression node)
        {
            var query = node.As<QueryExpression>();
            if (query != null)
            {
                foreach (var item in query.Projections.Count == 0
                    ? query.GenerateTableColumns()
                    : query.Projections.Select(item => item.Expression).OfType<ColumnExpression>())
                {
                    AddProjection(item.Member, item);
                }

                return node;
            }

            var col = node.As<ColumnExpression>();
            if (col != null)
            {
                AddProjection(col.Member, node);
            }

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInit)
        {
            Visit(memberInit.NewExpression);

            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                AddProjection(binding.Member, binding.Expression);
            }

            return memberInit;
        }

        protected override Expression VisitNew(NewExpression newExp)
        {
            if (newExp.Arguments == null || newExp.Arguments.Count == 0)
            {
                return newExp;
            }

            for (var i = 0; i < newExp.Arguments.Count; i++)
            {
                if (ReflectionUtil.SimpleTypes.Contains(newExp.Members[i].GetPropertyType()))
                {
                    AddProjection(newExp.Members[i], newExp.Arguments[i]);
                    continue;
                }

                if (newExp.Arguments[i].Is<QueryExpression>())
                {
                    var query = newExp.Arguments[i].As<QueryExpression>();
                    foreach (var item in query.Projections.Count == 0
                        ? query.GenerateTableColumns()
                        : query.Projections.Select(item => item.Expression).OfType<ColumnExpression>())
                    {
                        AddProjection(item.Member, item);
                    }
                }
            }

            return newExp;
        }

        private void AddProjection(MemberInfo member, Expression node)
        {
            if (node.NodeType != ExpressionType.Extension)
            {
                Visit(node);
                return;
            }

            var query = node.As<QueryExpression>();
            if (query != null)
            {
                foreach (var item in query.GenerateTableColumns())
                {
                    AddProjection(item.Member, item);
                }

                return;
            }

            var col = node.As<ColumnExpression>();
            if (col != null)
            {
                col.Member = member ?? col.Member;
                _columns.Add(new ColumnDescriptor()
                {
                    Member = member,
                    Expression = node,
                    Ordinal = _columns.Count
                });
            }
        }
    }
}
