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
                    return VisitQuery(query);
                case WhereExperssion where:
                    return VisitWhere(where);
                case ColumnExpression column:
                    return VisitColumn(column);
                case JoinExpression join:
                    return VisitJoin(join);
                case OrderExpression order:
                    return VisitOrder(order);
                case GroupExpression group:
                    return VisitGroup(group);
                case UnionExpression union:
                    return VisitUnion(union);
                case ExceptExpression except:
                    return VisitExcept(except);
                case IntersectExpression intersect:
                    return VisitIntersect(intersect);
                case SqlFunctionExpression function:
                    return VisitSqlFunction(function);
                case AggregateExpression aggregate:
                    return VisitAggregate(aggregate);
                case SkipExpression skip:
                    return VisitSkip(skip);
                case AllExpression all:
                    return VisitAll(all);
                case AnyExpression any:
                    return VisitAny(any);
                case InsertExpression insert:
                    return VisitInsert(insert);
                case UpdateExpression update:
                    return VisitUpdate(update);
                case DeleteExpression delete:
                    return VisitDelete(delete);
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

        protected abstract Expression VisitInsert(InsertExpression insert);

        protected abstract Expression VisitUpdate(UpdateExpression update);

        protected abstract Expression VisitDelete(DeleteExpression delete);

        public abstract string Build(Expression expression);
    }
}
