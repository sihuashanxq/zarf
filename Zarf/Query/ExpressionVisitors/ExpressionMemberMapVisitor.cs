using System;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionVisitors
{
    public class ExpressionMemberMapVisitor : ExpressionVisitor
    {
        private Func<QueryExpression, QueryExpression, IQueryContext, Expression> _makeObjectNew;

        private QueryExpression _rootQuery;

        private QueryContext _queryContext;

        public ExpressionMemberMapVisitor(
            QueryExpression rootQuery,
            Func<QueryExpression, QueryExpression, IQueryContext, Expression> makeObjectNew,
            QueryContext context)
        {
            _rootQuery = rootQuery;
            _makeObjectNew = makeObjectNew;
            _queryContext = context;
        }

        public override Expression Visit(Expression node)
        {
            if (node.NodeType == ExpressionType.Extension)
            {
                if (node.Is<QueryExpression>())
                {
                    return _makeObjectNew(_rootQuery, node.Cast<QueryExpression>(),  _queryContext);
                }
                else
                {
                    var ordinal = QueryUtils.FindProjectionOrdinal(_rootQuery, node);
                    if (ordinal == -1)
                    {
                        throw new Exception("ExpressionMemberMapVisitor");
                    }

                    _queryContext.ProjectionMappingProvider.Map(node, _rootQuery, ordinal);
                    return node;
                }
            }

            return base.Visit(node);
        }
    }
}
