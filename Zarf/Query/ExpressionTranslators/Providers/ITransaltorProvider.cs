using System.Linq.Expressions;

namespace Zarf.Query.ExpressionTranslators
{
    public interface ITransaltorProvider
    {
        ITranslaor GetTranslator(Expression node);
    }
}
