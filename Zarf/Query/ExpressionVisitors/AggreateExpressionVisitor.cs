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

            QueryExpression query = null;

            if (queryModel != null)
            {
                var modelExpression = queryModel.GetModelExpression(mem.Expression.Type.GetModelElementType());
                var refrence = Context.MemberBindingMapper.GetMapedExpression(Expression.MakeMemberAccess(modelExpression, mem.Member));
                if (refrence != null)
                {
                    query = Context.ProjectionOwner.GetQuery(refrence);
                }
            }

            if (query == null)
            {
                query = expression.As<ColumnExpression>()?.Query;
            }

            if (query == null || Query.ConstainsQuery(query))
            {
                return expression;
            }

            var cQuery = query.Clone();

            Query.AddProjection(expression);
            Query.Groups.Add(new GroupExpression(new[] { expression.As<ColumnExpression>() }));

            if (!HandledQueries.Contains(query))
            {
                Query.AddJoin(new JoinExpression(cQuery, null, JoinType.Cross));
                HandledQueries.Add(query);
            }

            return expression;
        }
    }
}
