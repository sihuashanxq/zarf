using System.Linq.Expressions;

namespace Zarf.Queries.ExpressionVisitors
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

            return base.Visit(node);
        }
    }
}
