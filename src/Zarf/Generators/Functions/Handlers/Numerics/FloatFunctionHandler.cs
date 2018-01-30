using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Numerics
{
    public class FloatFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(float);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS real )");
        }
    }

    public class FloatNullFunctionHandler : FloatFunctionHandler
    {
        public override Type SoupportedType => typeof(float?);
    }
}
