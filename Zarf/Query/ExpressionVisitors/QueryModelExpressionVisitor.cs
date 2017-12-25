using System.Linq.Expressions;

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
            if (queryModel != null)
            {
                return queryModel.Model;
            }

            return base.Visit(node);
        }
    }
}
