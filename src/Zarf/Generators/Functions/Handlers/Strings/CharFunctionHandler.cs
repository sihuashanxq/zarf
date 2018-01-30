using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Strings
{
    public class CharFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(char);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS NVARCHAR(1) ) ");
        }
    }

    public class CharNullFunctionHandler : CharFunctionHandler
    {
        public override Type SoupportedType => typeof(char?);
    }
}
