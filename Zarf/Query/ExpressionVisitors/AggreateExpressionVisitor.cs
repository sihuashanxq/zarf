using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public class AggreateExpressionVisitor : QueryCompiler
    {
        /// <summary>
        /// 已被处理的查询
        /// </summary>
        protected List<QueryExpression> HandledQueries { get; }

        public QueryExpression Query { get; }

        public AggreateExpressionVisitor(IQueryContext context, QueryExpression query) : base(context)
        {
            Query = query;
            HandledQueries = new List<QueryExpression>();
        }

        public override Expression Visit(Expression exp)
        {
            if (exp.NodeType == ExpressionType.MemberAccess)
            {
                return VisitMember(exp.As<MemberExpression>());
            }

            return base.Visit(exp);
        }

        /// <summary>
        /// 聚合引用其他表的列,拷贝引用表
        /// </summary>
        protected override Expression VisitMember(MemberExpression mem)
        {
            var expression = base.Visit(mem);
            var queryModel = Context.QueryModelMapper.GetQueryModel(mem.Expression);
            var query = queryModel.Query ?? expression.As<ColumnExpression>()?.Query;

            if (query != null && !Query.ConstainsQuery(query))
            {
                var cQuery = query.Clone();

                if (!HandledQueries.Contains(query))
                {
                    Query.AddJoin(new JoinExpression(cQuery, null, JoinType.Cross));
                    HandledQueries.Add(query);
                }

                //聚合函数不能有别名
                if (expression is ColumnExpression column)
                {
                    Query.AddProjection(column);
                    Query.Groups.Add(new GroupExpression(new[] { column }));
                    return column;
                }

                //聚合函数不能有别名
                if (expression is AliasExpression alias)
                {
                    Query.AddProjection(alias.Expression);
                    Query.Groups.Add(new GroupExpression(new[] { alias.Expression.As<ColumnExpression>() }));
                    return alias.Expression;
                }
            }

            return expression.As<AliasExpression>()?.Expression ?? expression;
        }
    }
}
