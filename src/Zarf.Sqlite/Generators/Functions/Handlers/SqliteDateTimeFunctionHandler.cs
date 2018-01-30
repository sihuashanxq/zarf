using System;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Generators;
using Zarf.Generators.Functions.Handlers.Dates;

namespace Zarf.Sqlite.Generators.Functions.Handlers
{
    public class SqliteDateTimeFunctionHandler : DateTimeFunctionHandler
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

            generator.Attach(" DateTimeAddTicks(");
            generator.Attach(methodCall.Object);
            generator.Attach(",");
            generator.Attach(Expression.Constant(timeSpan.TotalMilliseconds));
            generator.Attach("*1000");
            generator.Attach(")");
        }

        protected virtual void HandleAddTicks(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" DateTimeAddTicks(");
            generator.Attach(methodCall.Object);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(")");
        }

        protected virtual void HandleAddMilliseconds(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" DateTimeAddTicks(");
            generator.Attach(methodCall.Object);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach("*1000");
            generator.Attach(")");
        }

        protected virtual void HandleAddDays(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "days");
        }

        protected virtual void HandleAddHours(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "hours");
        }

        protected virtual void HandleAddMinutes(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "minutes");
        }

        protected virtual void HandleAddMonths(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "months");
        }

        protected virtual void HandleAddSeconds(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "seconds");
        }

        protected virtual void HandleAddYears(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            AddModifier(
                generator,
                methodCall.Object,
                methodCall.Arguments[0],
                "years");
        }

        protected void AddModifier(ISQLGenerator generator, Expression dateTime, Expression timeSpan, string modifier)
        {
            generator.Attach(" DATETIME(");
            generator.Attach(dateTime);
            generator.Attach(",'+");
            generator.Attach(Expression.Constant(modifier));
            generator.Attach(" ");
            generator.Attach(timeSpan);
            generator.Attach("'");
            generator.Attach(")");
        }
    }
}
