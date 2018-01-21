using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Queries.Expressions;

namespace Zarf.Queries.ExpressionVisitors
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
            var queryModel = QueryContext.QueryModelMapper.GetQueryModel(mem.Expression);

            if (queryModel != null)
            {
                if (!Query.ConstainsQuery(queryModel.Query))
                {
                    throw new NotImplementedException("can not aggregate a column from outer refrence!");
                }

                while (queryModel != null)
                {
                    if (queryModel.Model.Type != mem.Member.DeclaringType)
                    {
                        queryModel = queryModel.Previous;
                    }

                    var memberExpression = Expression.MakeMemberAccess(queryModel.Model, mem.Member);
                    var binding = QueryContext.MemberBindingMapper.GetMapedExpression(memberExpression);

                    if (binding == null)
                    {
                        queryModel = queryModel?.Previous;
                        continue;
                    }

                    if (binding is AliasExpression alis)
                    {
                        if (!(alis.Expression is QueryExpression))
                        {
                            return alis.Expression;
                        }

                        //引用为表,则说明这是子查询中的聚合
                        return new ColumnExpression(Query, new Column(alis.Alias), alis.Type);
                    }
                    else
                    {
                        return binding;
                    }

                    throw new Exception("find aggregage refrence colum faild!");
                }
            }

            var expression = base.Visit(mem);

            return expression.As<AliasExpression>()?.Expression ?? expression;
        }
    }
}
