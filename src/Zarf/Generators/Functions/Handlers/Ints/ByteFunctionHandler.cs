using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class ByteFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(byte);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS TINYINT ) ");
        }
    }

    public class ByteNullFunctionHandler : ByteFunctionHandler
    {
        public override Type SoupportedType => typeof(byte?);
    }
}
