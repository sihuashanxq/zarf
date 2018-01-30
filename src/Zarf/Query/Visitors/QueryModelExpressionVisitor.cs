using System.Linq.Expressions;

namespace Zarf.Query.Visitors
{
    /// <summary>
    /// QueryModel 替换掉中间某些部分
    /// </summary>
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

            var queryModel = Context.ModelMapper.GetValue(node);
            if (queryModel != null && queryModel.Model.Type == node.Type)
            {
                return queryModel.Model;
            }

            return base.Visit(node);
        }
    }
}
