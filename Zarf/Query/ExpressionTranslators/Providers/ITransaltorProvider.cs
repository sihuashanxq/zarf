using System.Linq.Expressions;

namespace Zarf.Queries.ExpressionTranslators
{
    public interface ITransaltorProvider
    {
        ITranslaor GetTranslator(Expression node);
    }
}
