using System;
using System.Linq;

namespace Zarf.Generators.Functions.Handlers.Collections
{
    public class EnumerableFunctionHandler : CollectionFunctionHandler
    {
        public override Type SoupportedType => typeof(Enumerable);
    }
}
