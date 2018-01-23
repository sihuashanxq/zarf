using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Metadata.Entities;
using Zarf.Query.Expressions;
using Zarf.Update.Expressions;

namespace Zarf.Generators
{
    public abstract class SQLGenerator : ExpressionVisitor, ISQLGenerator
    {
        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SelectExpression select:
                    VisitQuery(select);
                    break;
                case WhereExperssion where:
                    VisitWhere(where);
                    break;
                case ColumnExpression column:
                    VisitColumn(column);
                    break;
                case JoinExpression join:
                    VisitJoin(join);
                    break;
                case OrderExpression order:
                    VisitOrder(order);
                    break;
                case GroupExpression group:
                    VisitGroup(group);
                    break;
                case UnionExpression union:
                    VisitUnion(union);
                    break;
                case ExceptExpression except:
                    VisitExcept(except);
                    break;
                case IntersectExpression intersect:
                    VisitIntersect(intersect);
                    break;
                case AggregateExpression aggregate:
                    VisitAggregate(aggregate);
                    break;
                case SkipExpression skip:
                    VisitSkip(skip);
                    break;
                case AllExpression all:
                    VisitAll(all);
                    break;
                case AnyExpression any:
                    VisitAny(any);
                    break;
                case ExistsExpression exists:
                    VisitExists(exists);
                    break;
                case DbStoreExpression store:
                    VisitStore(store);
                    break;
                case AliasExpression alias:
                    VisitAlias(alias);
                    break;
            }

            return node;
        }

        protected abstract Expression VisitAlias(AliasExpression alias);

        protected abstract Expression VisitQuery(SelectExpression select);

        protected abstract Expression VisitWhere(WhereExperssion where);

        protected abstract Expression VisitColumn(ColumnExpression column);

        protected abstract Expression VisitJoin(JoinExpression join);

        protected abstract Expression VisitOrder(OrderExpression order);

        protected abstract Expression VisitGroup(GroupExpression group);

        protected abstract Expression VisitUnion(UnionExpression union);

        protected abstract Expression VisitExcept(ExceptExpression except);

        protected abstract Expression VisitIntersect(IntersectExpression intersec);

        protected abstract Expression VisitAggregate(AggregateExpression aggregate);

        protected abstract Expression VisitSkip(SkipExpression skip);

        protected abstract Expression VisitAll(AllExpression all);

        protected abstract Expression VisitAny(AnyExpression any);

        protected abstract Expression VisitExists(ExistsExpression exists);

        protected abstract Expression VisitStore(DbStoreExpression store);

        public abstract string Generate(Expression expression, List<DbParameter> parameters);

        public abstract string Generate(Expression expression);
    }
}
