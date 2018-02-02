using System.Linq.Expressions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Visitors
{
    /// <summary>
    /// 聚合被合并后转换内外层查询关联关系
    /// </summary>
    public class AggregateRelationExpressionVisitor : ExpressionVisitorBase
    {
        public IQueryContext QueryContext { get; }

        public SelectExpression Select { get; }

        public AggregateRelationExpressionVisitor(IQueryContext queryContext, SelectExpression select)
        {
            QueryContext = queryContext;
            Select = select;
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            if (Select == column.Select || Select.ChildSelect == column.Select)
            {
                return column;
            }

            //在ModelRewriter中被映射的列
            //移除Alias
            var key = QueryContext.ExpressionMapper.GetKey(column);
            if (key != null)
            {
                if (key is ColumnExpression col)
                {
                    col.Alias = string.Empty;
                }

                return key;
            }

            return column;
        }

        protected virtual Expression VisitSelect(SelectExpression exp)
        {
            if (exp.Where != null)
            {
                exp.Where = Visit(exp.Where) as WhereExperssion;
            }

            return exp;
        }

        protected virtual Expression VisitWhere(WhereExperssion exp)
        {
            if (exp.Predicate == null)
            {
                return exp;
            }

            return new WhereExperssion(Visit(exp.Predicate));
        }

        protected virtual Expression VisitAll(AllExpression exp)
        {
            VisitSelect(exp.Select);
            return exp;
        }

        protected virtual Expression VisitAny(AnyExpression exp)
        {
            VisitSelect(exp.Select);
            return exp;
        }

        protected virtual Expression VisitCaseWhen(CaseWhenExpression exp)
        {
            var @case = Visit(exp.CaseWhen);
            if (@case == exp.CaseWhen)
            {
                return exp;
            }

            return new CaseWhenExpression(@case, exp.Then, exp.Else, exp.Type);
        }

        protected virtual Expression VisitAlias(AliasExpression exp)
        {
            return new AliasExpression(exp.Alias, Visit(exp.Expression), null, exp.Type);
        }

        protected virtual Expression VisitExists(ExistsExpression exp)
        {
            Visit(exp.Select);
            return exp;
        }

        protected override Expression VisitExtension(Expression extension)
        {
            if (extension is ColumnExpression column)
            {
                return VisitColumn(column);
            }

            if (extension is WhereExperssion where)
            {
                return VisitWhere(where);
            }

            if (extension is SelectExpression select)
            {
                return VisitSelect(select);
            }

            if (extension is AllExpression all)
            {
                return VisitAll(all);
            }

            if (extension is AnyExpression any)
            {
                return VisitAny(any);
            }

            if (extension is CaseWhenExpression caseWhen)
            {
                return VisitCaseWhen(caseWhen);
            }

            if (extension is AliasExpression alias)
            {
                return VisitAlias(alias);
            }

            if (extension is ExistsExpression exists)
            {
                return VisitExists(exists);
            }

            return extension;
        }
    }
}
