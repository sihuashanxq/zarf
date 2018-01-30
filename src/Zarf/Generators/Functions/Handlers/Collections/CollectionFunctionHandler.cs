using System;
using System.Collections;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Generators.Functions.Handlers.Collections
{
    public class CollectionFunctionHandler : ISQLFunctionHandler
    {
        public virtual Type SoupportedType => typeof(IEnumerable);

        public virtual bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            if (!SoupportedType.IsAssignableFrom(methodCall.Method.DeclaringType))
            {
                return false;
            }

            if (methodCall.Method.Name == "Contains")
            {
                HandleContains(generator, methodCall);
                return true;
            }

            return false;
        }

        protected virtual void HandleContains(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            IEnumerable items = methodCall.Object != null
                ? methodCall.Object.As<ConstantExpression>()?.Value?.As<IEnumerable>()
                : methodCall.Arguments[0].As<ConstantExpression>().Value?.As<IEnumerable>();

            if (items == null)
            {
                throw new NullReferenceException("collecton is null!");
            }

            generator.Attach(methodCall.Arguments.Count == 2
                ? methodCall.Arguments[1]
                : methodCall.Arguments[0]);

            generator.Attach("  IN (");

            var first = true;

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                generator.Attach(Expression.Constant(item));

                if (!first)
                {
                    generator.Attach(",");
                }

                first = false;
            }

            generator.Attach(") ");
        }
    }
}
