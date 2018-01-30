using System.Linq.Expressions;
using Zarf.Generators;
using Zarf.Generators.Functions.Handlers.Strings;

namespace Zarf.Sqlite.Generators.Functions.Handlers
{
    public class SqliteStringFunctionHandler : StringFunctionHandler
    {
        protected override void HandleSubstring(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" SubStr(");
            generator.Attach(methodCall.Object);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[0]);

            if (methodCall.Arguments.Count == 2)
            {
                generator.Attach(",");
                generator.Attach(methodCall.Arguments[1]);
                generator.Attach(")");
                return;
            }

            //SUBSTRING(Name,0,LEN(NAME)-0)
            generator.Attach(",(LEN ( ");
            generator.Attach(methodCall.Object);
            generator.Attach(")-");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(")");
        }
    }
}
