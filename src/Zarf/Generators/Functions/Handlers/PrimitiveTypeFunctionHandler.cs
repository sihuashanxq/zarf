using System;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Generators.Functions.Handlers
{
    /// <summary>
    /// 基本类型函数处理
    /// 只包括Equlas 和Parse
    /// </summary>
    public abstract class PrimitiveTypeFunctionHandler : ISQLFunctionHandler
    {
        public abstract Type SoupportedType { get; }

        public virtual bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            if (methodCall.Method.DeclaringType != SoupportedType)
            {
                return false;
            }

            if (methodCall.Method.Name == "Parse")
            {
                HandleParse(generator, methodCall);
                return true;
            }

            if (methodCall.Method.Name == "Equals")
            {
                HandleEquals(generator, methodCall);
                return true;
            }

            if (methodCall.Method.Name == "ToString")
            {
                HandleToString(generator, methodCall);
                return true;
            }

            return false;
        }

        protected abstract void HandleParse(ISQLGenerator generator, MethodCallExpression methodCall);

        protected virtual void HandleEquals(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments[0].IsNullValueConstant() && methodCall.Arguments[1].IsNullValueConstant())
            {
                generator.Attach(" 1 = 1 ");
                return;
            }

            if (methodCall.Arguments[0].IsNullValueConstant())
            {
                generator.Attach(methodCall.Arguments[1]);
                generator.Attach(" IS NULL ");
                return;
            }

            if (methodCall.Arguments[1].IsNullValueConstant())
            {
                generator.Attach(methodCall.Arguments[0]);
                generator.Attach(" IS NULL ");
                return;
            }

            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" = ");
            generator.Attach(methodCall.Arguments[1]);
        }

        protected virtual void HandleToString(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CAST (");
            generator.Attach(methodCall.Object);
            generator.Attach(" AS NVARCHAR )");
        }
    }
}
