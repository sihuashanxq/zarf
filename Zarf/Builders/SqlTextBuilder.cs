using System.Linq.Expressions;
using Zarf.Query.Expressions;
using System;
using System.Collections.Generic;

namespace Zarf.Builders
{
    public abstract class SqlTextBuilder : ExpressionVisitor, ISqlTextBuilder
    {
        protected static HashSet<Type> NumbericTypes = new HashSet<Type>()
        {
            typeof(Int32),
            typeof(Int16),
            typeof(Int64),
            typeof(UInt16),
            typeof(UInt32),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal)
        };

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case QueryExpression query:
                    node = VisitQuery(query);
                    break;
                case WhereExperssion where:
                    node = VisitWhere(where);
                    break;
                case ColumnExpression column:
                    node = VisitColumn(column);
                    break;
                case JoinExpression join:
                    node = VisitJoin(join);
                    break;
                case OrderExpression order:
                    node = VisitOrder(order);
                    break;
                case GroupExpression group:
                    node = VisitGroup(group);
                    break;
                case UnionExpression union:
                    node = VisitUnion(union);
                    break;
                case ExceptExpression except:
                    node = VisitExcept(except);
                    break;
                case IntersectExpression intersect:
                    node = VisitIntersect(intersect);
                    break;
                case SqlFunctionExpression function:
                    node = VisitSqlFunction(function);
                    break;
                case AggregateExpression aggregate:
                    node = VisitAggregate(aggregate);
                    break;
                case SkipExpression skip:
                    node = VisitSkip(skip);
                    break;
                case AllExpression all:
                    node = VisitAll(all);
                    break;
                case AnyExpression any:
                    node = VisitAny(any);
                    break;
            }
            return node;
        }

        protected abstract Expression VisitQuery(QueryExpression query);

        protected abstract Expression VisitWhere(WhereExperssion where);

        protected abstract Expression VisitColumn(ColumnExpression column);

        protected abstract Expression VisitJoin(JoinExpression join);

        protected abstract Expression VisitOrder(OrderExpression order);

        protected abstract Expression VisitGroup(GroupExpression group);

        protected abstract Expression VisitUnion(UnionExpression union);

        protected abstract Expression VisitExcept(ExceptExpression except);

        protected abstract Expression VisitIntersect(IntersectExpression intersec);

        protected abstract Expression VisitSqlFunction(SqlFunctionExpression function);

        protected abstract Expression VisitAggregate(AggregateExpression aggregate);

        protected abstract Expression VisitSkip(SkipExpression skip);

        protected abstract Expression VisitAll(AllExpression all);

        protected abstract Expression VisitAny(AnyExpression any);

        public abstract string Build(Expression expression);
    }
}
