using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Query
{
    public static class ExpressionVisitorExtensions
    {
        public static IEnumerable<ElementInit> VisitElementInits(this ExpressionVisitor expressionVisitor, IEnumerable<ElementInit> elementInits)
        {
            foreach (var init in elementInits)
            {
                var arguments = expressionVisitor.Visit(init.Arguments);
                if (arguments != init.Arguments)
                {
                    yield return init.Update(arguments);
                }
                else
                {
                    yield return init;
                }
            }
        }
    }
}
