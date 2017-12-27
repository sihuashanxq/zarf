using System.Linq.Expressions;
using Zarf.Extensions;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Zarf.Query.ExpressionVisitors
{
    public class QueryModelExpressionVisitor : ExpressionVisitorBase
    {
        public IQueryContext Context { get; }

        public QueryModelExpressionVisitor(IQueryContext context)
        {
            Context = context;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            var queryModel = Context.QueryModelMapper.GetQueryModel(node);
            if (queryModel != null &&
                queryModel.Model.Type == node.Type)
            {
                return queryModel.Model;
            }

            if (queryModel != null && queryModel.Model.Type == node.Type.GetModelElementType())
            {
                var x = Expression.ElementInit(typeof(List<>).MakeGenericType(queryModel.Model.Type).GetMethod("Add"), queryModel.Model);
                var con = typeof(List<>).MakeGenericType(queryModel.Model.Type).GetConstructor(new Type[0]);
                return Expression.ListInit(Expression.New(con), x);
            }

            return base.Visit(node);
        }
    }
}
