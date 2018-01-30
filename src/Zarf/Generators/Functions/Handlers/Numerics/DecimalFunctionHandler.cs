using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Numerics
{
    public class DecimalFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(decimal);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS NUMERIC(38,18) )");
        }
    }

    public class DecimalNullFunctionHandler : DecimalFunctionHandler
    {
        public override Type SoupportedType => typeof(decimal?);
    }
}
