using System.Linq.Expressions;

namespace Zarf.Query.Handlers
{
    public interface IQueryNodeHandler<in TNode> : IQueryNodeHandler
    {
        Expression HandleNode(TNode node);
    }

    public interface IQueryNodeHandler
    {
        IQueryCompiler QueryCompiler { get; }

        IQueryContext QueryContext { get; }

        Expression HandleNode(Expression node);
    }
}
