using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;

namespace Zarf.Generators.Functions
{
    public class MathFunctionHandler : ISQLFunctionHandler
    {
        public Type SoupportedType { get; }

        public MathFunctionHandler()
        {
            SoupportedType = typeof(Math);
        }

        public bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            if (SoupportedType != methodCall.Method.DeclaringType)
            {
                return false;
            }

            switch (methodCall.Method.Name)
            {
                case "Abs":
                    HandleAbs(generator, methodCall);
                    break;
                case "Acos":
                    HandleAcos(generator, methodCall);
                    break;
                case "Asin":
                    HandleAsin(generator, methodCall);
                    break;
                case "Atan":
                    HandleAtan(generator, methodCall);
                    break;
                case "Cos":
                    HandleCos(generator, methodCall);
                    break;
                case "Ceiling":
                    HandleCeiling(generator, methodCall);
                    break;
                case "Floor":
                    HandleFloor(generator, methodCall);
                    break;
                case "Exp":
                    HandleExp(generator, methodCall);
                    break;
                case "Log10":
                    HandleLog10(generator, methodCall);
                    break;
                case "Sign":
                    HandleSign(generator, methodCall);
                    break;
                case "Sin":
                    HandleSin(generator, methodCall);
                    break;
                case "Sqrt":
                    HandleSqrt(generator, methodCall);
                    break;
                case "Tan":
                    HandleTan(generator, methodCall);
                    break;
                case "Atan2":
                    HandleAtan2(generator, methodCall);
                    break;
                case "Log":
                    HandleLog(generator, methodCall);
                    break;
                case "Max":
                    HandleMax(generator, methodCall);
                    break;
                case "Min":
                    HandleMin(generator, methodCall);
                    break;
                case "Pow":
                    HandlePow(generator, methodCall);
                    break;
                case "Round":
                    HandleRound(generator, methodCall);
                    break;
                case "Sinh":
                    HandleSinh(generator, methodCall);
                    break;
                case "Cosh":
                    HandleCosh(generator, methodCall);
                    break;
                case "Tanh":
                    HandleTanh(generator, methodCall);
                    break;
                case "Truncate":
                    HandleTruncate(generator, methodCall);
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected virtual void HandleAbs(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleAcos(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleAsin(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleAtan(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleCos(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleCeiling(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleFloor(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleExp(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleLog10(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleSign(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleSin(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleSqrt(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleTan(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleDefaultOneArgumnetFunction(generator, methodCall);
        }

        protected virtual void HandleAtan2(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("Atn2(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(")");
        }

        protected virtual void HandleLog(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("LOG(");
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

        protected virtual void HandleMax(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("CASE WHEN ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" > ");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(" THEN ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" ELSE  ");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(" END ");
        }

        protected virtual void HandleMin(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("CASE WHEN ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" < ");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(" THEN ");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" ELSE  ");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(" END ");
        }

        protected virtual void HandlePow(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("POWER(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",");
            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(")");
        }

        protected virtual void HandleRound(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach("Round(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",");

            if (methodCall.Arguments.Count == 1)
            {
                generator.Attach("0");
                generator.Attach(")");
                return;
            }

            if (methodCall.Arguments.Count == 2)
            {
                generator.Attach(methodCall.Arguments[1]);
                generator.Attach(")");
                return;
            }

            generator.Attach(methodCall.Arguments[1]);
            generator.Attach(",");
            generator.Attach(((int)methodCall.Arguments[2].Cast<ConstantExpression>().Value.Cast<MidpointRounding>()).ToString());
            generator.Attach(")");
        }

        protected virtual void HandleSinh(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            var v = methodCall.Arguments[0].Cast<ConstantExpression>().Value.Cast<double>();

            generator.Attach("(( POWER(EXP(1),");
            generator.Attach(v.ToString());
            generator.Attach(")");
            generator.Attach("- POWER(EXP(1),");
            generator.Attach((-v).ToString());
            generator.Attach(") )");
            generator.Attach("/2.0)");
        }

        protected virtual void HandleCosh(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            var v = methodCall.Arguments[0].Cast<ConstantExpression>().Value.Cast<double>();

            generator.Attach("(( POWER(EXP(1),");
            generator.Attach(v.ToString());
            generator.Attach(")");
            generator.Attach("+ POWER(EXP(1),");
            generator.Attach((-v).ToString());
            generator.Attach(") )");
            generator.Attach("/2.0)");
        }

        protected virtual void HandleTanh(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            HandleSinh(generator, methodCall);
            generator.Attach("/");
            HandleCosh(generator, methodCall);
        }

        protected virtual void HandleTruncate(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(" CASE WHEN Round(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",0)>");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(" THEN ROUND(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",0)-1 ELSE  ROUND(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(",0) END");
        }

        protected void HandleDefaultOneArgumnetFunction(ISQLGenerator generator, MethodCallExpression methodCall)
        {
            generator.Attach(methodCall.Method.Name);
            generator.Attach("(");
            generator.Attach(methodCall.Arguments[0]);
            generator.Attach(")");
        }
    }
}
