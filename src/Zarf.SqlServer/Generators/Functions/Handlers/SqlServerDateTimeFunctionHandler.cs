using System;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Generators;
using Zarf.Generators.Functions.Handlers.Dates;

namespace Zarf.SqlServer.Generators.Functions.Handlers
{
    public class SqlServerDateTimeFunctionHandler : DateTimeFunctionHandler
    {
        public override bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            if (methodCall.Method.DeclaringType != SoupportedType)
            {
                return false;
            }

            switch (methodCall.Method.Name)
            {
                case "Add":
                    HandleAdd(generator, methodCall);
                    return true;
                case "AddTicks":
                    HandleAddTicks(generator, methodCall);
                    return true;
                case "AddDays":
                    HandleAddDays(generator, methodCall);
                    return true;
                case "AddHours":
                    HandleAddHours(generator, methodCall);
                    return true;
                case "AddMilliseconds":
                    HandleAddMilliseconds(generator, methodCall);
                    return true;
                case "AddMinutes":
                    HandleAddMinutes(generator, methodCall);
                    return true;
                case "AddMonths":
                    HandleAddMonths(generator, methodCall);
                    return true;
                case "AddSeconds":
                    HandleAddSeconds(generator, methodCall);
                    return true;
                case "AddYears":
                    HandleAddYears(generator, methodCall);
                    return true;
            }

            return base.HandleFunction(generator, methodCall);
        }

        protected virtual void HandleAdd(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            var timeSpan = methodCall.Arguments[0].Cast<ConstantExpression>().GetValue<TimeSpan>();

            AddModifier(
                generator,
                methodCall.Object,
                Expression.Constant(timeSpan.TotalMilliseconds),
                "MS");
        }

        protected virtual void HandleAddTicks(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "NS");
        }

        protected virtual void HandleAddDays(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "DD");
        }

        protected virtual void HandleAddHours(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "HH");
        }

        protected virtual void HandleAddMilliseconds(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "MS");
        }

        protected virtual void HandleAddMinutes(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "MI");
        }

        protected virtual void HandleAddMonths(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "MM");
        }

        protected virtual void HandleAddSeconds(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "SS");
        }

        protected virtual void HandleAddYears(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "YY");
        }

        protected void AddModifier(ISQLGenerator generator, Expression dateTime, Expression timeSpan, string modifier)
        {
            generator.Attach(" DATEADD(");
            generator.Attach(Expression.Constant(modifier));
            generator.Attach(",");
            generator.Attach(timeSpan);
            generator.Attach(",");
            generator.Attach(dateTime);
            generator.Attach(")");
        }
    }
}
