using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Numerics
{
    public class DoubleFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(double);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS FLOAT )");
        }
    }

    public class DoubleNullFunctionHanlder : DoubleFunctionHandler
    {
        public override Type SoupportedType => typeof(double?);
    }
}
