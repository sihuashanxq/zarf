using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class IntFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(int);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS INT )");
        }
    }

    public class IntNullFunctionHandler : IntFunctionHandler
    {
        public override Type SoupportedType => typeof(int?);
    }
}
