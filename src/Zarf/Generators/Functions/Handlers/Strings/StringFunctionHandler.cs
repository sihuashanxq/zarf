using System;
using System.Linq.Expressions;

namespace Zarf.Generators.Functions.Handlers.Strings
{
    public class StringFunctionHandler : ISQLFunctionHandler
    {
        public virtual Type SoupportedType => typeof(string);

        public bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            if (SoupportedType != methodCall.Method.DeclaringType)
            {
                return false;
            }

            switch (methodCall.Method.Name)
            {
                case "IsNullOrEmpty":
                    HandleStringOrEmpty(generator, methodCall);
                    return true;
                case "IsNullOrWhiteSpace":
                    HandleStringOrWhiteSpace(generator, methodCall);
                    return true;
                case "StartsWith":
                    HandleStartsWith(generator, methodCall);
                    return true;
                case "EndsWith":
                    HandleEndsWith(generator, methodCall);
                    return true;
                case "Contains":
                    HandleContains(generator, methodCall);
                    return true;
                case "Trim":
                    HandleTrim(generator, methodCall);
                    return true;
                case "TrimStart":
                    HandleTrimStart(generator, methodCall);
                    return true;
                case "TrimEnd":
                    HandleTrimEnd(generator, methodCall);
                    return true;
                case "IndexOf":
                    HandleIndexOf(generator, methodCall);
                    return true;
                case "Substring":
                    HandleSubstring(generator, methodCall);
                    return true;
                case "ToLower":
                    HandleToLower(generator, methodCall);
                    return true;
                case "ToUpper":
                    HandleToUpper(generator, methodCall);
                    return true;
                case "Replace":
                    HandleReplace(generator, methodCall);
                    return true;
                case "Concat":
                    HandleConcat(generator, methodCall);
                    return true;
                default:
                    return false; ;
            }
        }

        protected virtual void HandleStartsWith(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(methodCall.Object);
            generator.Attach(" LIKE '%'+ ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" +'%'");
        }

        protected virtual void HandleEndsWith(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(methodCall.Object);
            generator.Attach(" LIKE '%'+ ");
            generator.Attach(methodCall.Arguments[0]);
        }

        protected virtual void HandleContains(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(methodCall.Object);
            generator.Attach(" LIKE '%'+ ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" +'%'");
        }

        protected virtual void HandleTrim(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" LTRIM(RTRIM(");
            generator.Attach(methodCall.Object);
            generator.Attach(" ))");
        }

        protected virtual void HandleTrimStart(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" LTRIM(");
            generator.Attach(methodCall.Object);
            generator.Attach(" )");
        }

        protected virtual void HandleTrimEnd(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" RTRIM(");
            generator.Attach(methodCall.Object);
            generator.Attach(" )");
        }

        protected virtual void HandleIndexOf(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("(CHARINDEX(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",");
            generator.Attach(methodCall.Object);
            generator.Attach(")-1)");
        }

        protected virtual void HandleSubstring(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" SUBSTRING(");
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

        protected virtual void HandleToLower(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" LOWER(");
            generator.Attach(methodCall.Object);
            generator.Attach(")");
        }

        protected virtual void HandleToUpper(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" Upper(");
            generator.Attach(methodCall.Object);
            generator.Attach(")");
        }

        protected virtual void HandleReplace(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" Replace(");
            generator.Attach(methodCall.Object);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(")");
        }

        protected virtual void HandleConcat(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            foreach (var argument in methodCall.Arguments)
            {
                if (argument != methodCall.Arguments[0])
                {
                    generator.Attach("+");
                }

                if (argument.NodeType != ExpressionType.Extension)
                {
                    generator.Attach("'");
                    generator.Attach(argument);
                    generator.Attach("'");
                    continue;
                }

                generator.Attach(argument);
            }
        }

        protected virtual void HandleToString(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(methodCall.Object);
        }

        protected virtual void HandleStringOrEmpty(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" ( ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" IS NULL OR   ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" = '' ) ");
        }

        protected virtual void HandleStringOrWhiteSpace(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" ( ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" IS NULL OR   LTRIM(RTRIM(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(")) = '' ) ");
        }
    }
}
