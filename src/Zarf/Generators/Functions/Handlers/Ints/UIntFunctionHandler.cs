using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class UIntFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(UInt32);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS BIGINT ) ");
        }
    }

    public class UIntNullFunctionHandler : UIntFunctionHandler
    {
        public override Type SoupportedType => typeof(UInt32?);
    }
}
