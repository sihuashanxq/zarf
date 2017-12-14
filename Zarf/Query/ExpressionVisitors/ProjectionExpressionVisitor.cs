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
            var col = node.As<ColumnExpression>();
            if (col != null)
            {
                AddProjection(col.Member, node);
                return node;
            }

            var query = node.As<QueryExpression>();
            if (query != null)
            {
                if (query.Columns.Count == 0)
                {
                    foreach (var item in query.GenerateTableColumns())
                    {
                        AddProjection(item.As<ColumnExpression>()?.Member, item);
                    }
                }
                else
                {
                    foreach (var item in query.Columns.Select(item => item.Expression))
                    {
                        if (item.Is<ColumnExpression>())
                        {
                            col = item.As<ColumnExpression>();
                            AddProjection(col.Member, col);
                        }
                        else if (item.Is<AggregateExpression>())
                        {
                            var key = item.As<AggregateExpression>().KeySelector;
                            if (key.Is<ColumnExpression>())
                            {
                                col = key.As<ColumnExpression>().Clone();
                                col.Query = query;
                                AddProjection(col.Member, col);
                            }
                            else
                            {
                                AddProjection(null, key);
                            }
                        }
                    }
                }
                return node;
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
                    Visit(newExp.Arguments[i]);
                }
            }

            return newExp;
        }

        private void AddProjection(MemberInfo member, Expression node)
        {
            if (node.NodeType != ExpressionType.Extension && node.NodeType != ExpressionType.Constant)
            {
                Visit(node);
                return;
            }

            var query = node.As<QueryExpression>();
            if (query != null)
            {
                Visit(query);
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
