using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Dates
{
    public class DateTimeFunctionHandler : PrimitiveTypeFunctionHandler
    {
        public override Type SoupportedType => typeof(DateTime);

        protected override void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" AS DATETIME ) ");
        }
    }

    public class DateTimeNullFunctionHandler : DateTimeFunctionHandler
    {
        public override Type SoupportedType => typeof(DateTime?);
    }
}
