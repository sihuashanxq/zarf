using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class ShortFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(Int16);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS SMALLINT )");
        }
    }

    public class ShortNullFunctionHandler : ShortFunctionHandler
    {
        public override Type SoupportedType => typeof(Int16?);
    }
}
