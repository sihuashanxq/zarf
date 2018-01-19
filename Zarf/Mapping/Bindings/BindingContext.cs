using System.Linq.Expressions;
using System;
using Zarf.Queries.Expressions;

namespace Zarf.Mapping.Bindings
{
    public class BindingContext : IBindingContext
    {
        public Expression Query { get; }

        public BindingContext(Expression query)
        {
            Query = query;
        }
    }
}
