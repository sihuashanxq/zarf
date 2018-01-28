using System.Linq.Expressions;
using Zarf.Generators;
using Zarf.Generators.Functions;

namespace Zarf.Sqlite.Generators.Functions.Handlers
{
    public class SqliteMathFunctionHandler : MathFunctionHandler
    {
        protected override void HandlePow(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("POW(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(")");
        }

        protected override void HandleAtan2(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("Atan2(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(")");
        }

        protected override void HandleLog(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(methodCall.Arguments.Count == 2 ? "LOG2(" : "LOG(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",");

            if (methodCall.Arguments.Count == 2)
            {
                generator.Attach(methodCall.Arguments[1]);
                generator.Attach(")");
                return;
            }

            generator.Attach("EXP(1)");
            generator.Attach(")");
        }
    }
}
