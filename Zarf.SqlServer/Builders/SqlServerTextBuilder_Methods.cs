using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Zarf.Builders;
using Zarf.Extensions;

namespace Zarf.SqlServer.Builders
{
    internal partial class SqlServerTextBuilder : SqlTextBuilder
    {
        protected static HashSet<string> LikeMethods = new HashSet<string>
        {
            "StartsWith","EndsWith","Contains"
        };

        protected static readonly Dictionary<string, string> MathFunctions = new Dictionary<string, string>()
        {
            { "Abs" ,"Abs"},
            { "Acos","Acos" },
            { "Asin","Asin" },
            { "Atan","Atan" },
            { "Atan2","Atn2"},
            { "Ceiling","Ceiling" },
            { "Cos","Cos"},
            { "Exp","Exp"},
            { "Floor","Floor"},
            { "Log","Log" },
            { "Log10","Log10" },
            { "Pow","Power"},
            { "Round","Round" },
            { "Sign","Sign"},
            { "Sin","Sin" },
            { "Sqrt","Sqrt" },
            { "Tan","Tan" }
        };

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (methodCall.Method.Name == "ToString")
            {
                BuildToString(methodCall);
                return methodCall;
            }

            if (BuildEquals(methodCall))
            {
                return methodCall;
            }

            if (methodCall.Method.DeclaringType == StringType)
            {
                BuildStringMethods(methodCall);
            }
            else if (methodCall.Method.DeclaringType == DateType)
            {
                BuildDateTimeMethods(methodCall);
            }
            else if (methodCall.Method.DeclaringType == MathType)
            {
                BuildMathFunction(methodCall);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(methodCall.Method.DeclaringType) || typeof(Enumerable).IsAssignableFrom(methodCall.Method.DeclaringType))
            {
                IEnumerable items;
                //list
                if (methodCall.Object != null)
                {
                    items = methodCall.Object.As<ConstantExpression>()?.Value?.As<IEnumerable>();
                }
                //array
                else
                {
                    items = methodCall.Arguments[0].As<ConstantExpression>().Value?.As<IEnumerable>();
                }

                if (items == null)
                {
                    throw new NullReferenceException("collecton is null!");
                }

                var sb = new StringBuilder();
                sb.Append("(");

                foreach (var item in items)
                {
                    if (item == null)
                        continue;

                    sb.Append('\'').Append(item.ToString()).Append("',");
                }

                sb.Length--;
                sb.Append(")");

                if (methodCall.Method.Name == "Contains")
                {
                    //array
                    if (methodCall.Arguments.Count == 2)
                    {
                        BuildExpression(methodCall.Arguments[1]);
                    }
                    else
                    {
                        BuildExpression(methodCall.Arguments[0]);
                    }

                    Append("  IN ");
                    Append(sb.ToString());
                }
            }

            return methodCall;
        }


        protected virtual bool BuildEquals(MethodCallExpression methodCall)
        {
            if (methodCall.Method.Name == "Equals")
            {
                BuildExpression(methodCall.Object);
                Append("=");
                BuildExpression(methodCall.Arguments[0]);
                return true;
            }

            return false;
        }

        protected virtual void BuildToString(MethodCallExpression methodCall)
        {
            Append("CAST(");
            BuildExpression(methodCall.Object);
            Append(" AS NVARCHAR )");
        }

        protected virtual void BuildStringMethods(MethodCallExpression methodCall)
        {
            switch (methodCall.Method.Name)
            {

                case "StartsWith":
                    BuildExpression(methodCall.Object);
                    Append(" LIKE '%'+ ");
                    BuildExpression(methodCall.Arguments[0]);
                    Append(" +'%'");
                    break;
                case "EndsWith":
                    BuildExpression(methodCall.Object);
                    Append(" LIKE '%'+ ");
                    BuildExpression(methodCall.Arguments[0]);
                    break;
                case "Contains":
                    BuildExpression(methodCall.Object);
                    Append(" LIKE '%'+ ");
                    BuildExpression(methodCall.Arguments[0]);
                    Append(" +'%'");
                    break;
                case "Trim":
                    Append(" LTRIM(RTRIM(");
                    BuildExpression(methodCall.Object);
                    Append(" ))");
                    break;
                case "TrimStart":
                    Append(" LTRIM(");
                    BuildExpression(methodCall.Object);
                    Append(')');
                    break;
                case "TrimEnd":
                    Append(" RTRIM(");
                    BuildExpression(methodCall.Object);
                    Append(')');
                    break;
                case "IndexOf":
                    Append("(CHARINDEX(");
                    BuildExpression(methodCall.Arguments[0]);
                    Append(',');
                    BuildExpression(methodCall.Object);
                    Append(")-1)");
                    break;
                case "Substring":
                    Append(" SUBSTRING(");
                    BuildExpression(methodCall.Object);
                    Append(',');
                    BuildExpression(methodCall.Arguments[0]);
                    if (methodCall.Arguments.Count == 2)
                    {
                        Append(',');
                        BuildExpression(methodCall.Arguments[1]);
                    }
                    else
                    {
                        //SUBSTRING(Name,0,LEN(NAME)-0)
                        Append(",(LEN ( ");
                        BuildExpression(methodCall.Object);
                        Append(")-");
                        BuildExpression(methodCall.Arguments[0]);
                        Append(")");
                    }
                    Append(')');
                    break;
                case "ToLower":
                    Append(" LOWER(");
                    BuildExpression(methodCall.Object);
                    Append(')');
                    break;
                case "ToUpper":
                    Append(" Upper(");
                    BuildExpression(methodCall.Object);
                    Append(')');
                    break;
                case "Replace":
                    Append(" Replace(");
                    BuildExpression(methodCall.Object);
                    Append(',');
                    BuildExpression(methodCall.Arguments[0]);
                    Append(',');
                    BuildExpression(methodCall.Arguments[1]);
                    Append(")");
                    break;
                case "Concat":
                    Append("CONCAT(");
                    foreach (var argument in methodCall.Arguments)
                    {
                        BuildExpression(argument);
                        Append(',');
                    }
                    Builder.Length--;
                    Append(')');
                    break;
                default:
                    throw new NotSupportedException($"{methodCall.Method.Name} is not supported!");
            }
        }

        protected virtual void BuildDateTimeMethods(MethodCallExpression methodCall)
        {
            switch (methodCall.Method.Name)
            {
                case "Add":
                    var timeSpan = methodCall.Arguments[0].Cast<ConstantExpression>().GetValue<TimeSpan>();
                    BuildDateAdd(methodCall.Object, Expression.Constant(timeSpan.TotalMilliseconds), "MS");
                    break;
                //DateTime2
                case "AddTicks":
                    BuildDateAdd(methodCall.Object, methodCall.Arguments[0], "NS");
                    break;
                case "AddDays":
                    BuildDateAdd(methodCall.Object, methodCall.Arguments[0], "DD");
                    break;
                case "AddHours":
                    BuildDateAdd(methodCall.Object, methodCall.Arguments[0], "HH");
                    break;
                case "AddMilliseconds":
                    BuildDateAdd(methodCall.Object, methodCall.Arguments[0], "MS");
                    break;
                case "AddMinutes":
                    BuildDateAdd(methodCall.Object, methodCall.Arguments[0], "MI");
                    break;
                case "AddMonths":
                    BuildDateAdd(methodCall.Object, methodCall.Arguments[0], "MM");
                    break;
                case "AddSeconds":
                    BuildDateAdd(methodCall.Object, methodCall.Arguments[0], "SS");
                    break;
                case "AddYears":
                    BuildDateAdd(methodCall.Object, methodCall.Arguments[0], "YY");
                    break;
            }
        }

        protected virtual void BuildDateAdd(Expression date, Expression span, string addType)
        {
            Append(" DATEADD(", addType, ',');
            BuildExpression(span);
            Append(',');
            BuildExpression(date);
            Append(')');
        }

        /// <summary>
        /// 构建数学函数 Math.XXX
        /// </summary>
        /// <param name="methodCall"></param>
        protected virtual void BuildMathFunction(MethodCallExpression methodCall)
        {
            var arguments = methodCall.Arguments;
            switch (methodCall.Method.Name)
            {
                case "Abs":
                case "Acos":
                case "Asin":
                case "Atan":
                case "Cos":
                case "Ceiling":
                case "Floor":
                case "Exp":
                case "Log10":
                case "Sign":
                case "Sin":
                case "Sqrt":
                case "Tan":
                    Append(MathFunctions[methodCall.Method.Name], '(');
                    BuildExpression(arguments[0]);
                    Append(')');
                    break;
                case "Atan2":
                    Append("Atn2(");
                    BuildExpression(arguments[0]);
                    Append(',');
                    BuildExpression(arguments[1]);
                    Append(")");
                    break;
                case "Log":
                    Append("LOG(");
                    BuildExpression(arguments[0]);
                    Append(',');

                    if (arguments.Count == 2)
                        BuildExpression(arguments[1]);
                    else
                        Append("EXP(1)");
                    Append(')');
                    break;
                case "Max":
                    BuildMathMax(methodCall);
                    break;
                case "Min":
                    BuildMathMin(methodCall);
                    break;
                case "Pow":
                    Append("POWER(");
                    BuildExpression(arguments[0]);
                    Append(',');
                    BuildExpression(arguments[1]);
                    Append(')');
                    break;
                case "Round":
                    BuildMathRound(methodCall);
                    break;
                case "Sinh":
                    BuildMathSinh(methodCall);
                    break;
                case "Cosh":
                    BuildMathCosh(methodCall);
                    break;
                case "Tanh":
                    BuildMathSinh(methodCall);
                    Builder.Append('/');
                    BuildMathCosh(methodCall);
                    break;
                case "Truncate":
                    BuildMathTruncate(methodCall);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 双曲正弦函数
        /// </summary>
        /// <param name="methodCall"></param>
        protected virtual void BuildMathSinh(MethodCallExpression methodCall)
        {
            var num = methodCall.Arguments[0].Cast<ConstantExpression>().Value.Cast<double>();
            Append("(( POWER(EXP(1),", num, ')');
            Append("- POWER(EXP(1),", -num, ") )");
            Append("/2.0)");
        }

        /// <summary>
        /// 双曲余弦函数
        /// </summary>
        /// <param name="methodCall"></param>
        protected virtual void BuildMathCosh(MethodCallExpression methodCall)
        {
            var num = methodCall.Arguments[0].Cast<ConstantExpression>().Value.Cast<double>();
            Append("(( POWER(EXP(1),", num, ')');
            Append("+ POWER(EXP(1),", -num, ") )");
            Append("/2.0)");
        }

        /// <summary>
        /// 数字取整
        /// </summary>
        /// <param name="methodCall"></param>
        protected virtual void BuildMathTruncate(MethodCallExpression methodCall)
        {
            Append(" CASE WHEN Round(");
            BuildExpression(methodCall.Arguments[0]);
            Append(",0)>");
            BuildExpression(methodCall.Arguments[0]);
            Append(" THEN ROUND(");
            BuildExpression(methodCall.Arguments[0]);
            Append(",0)-1 ELSE  ROUND(");
            BuildExpression(methodCall.Arguments[0]);
            Append(",0) END");
        }

        /// <summary>
        /// 最大值比较
        /// </summary>
        /// <param name="methodCall"></param>
        protected virtual void BuildMathMax(MethodCallExpression methodCall)
        {
            Append("CASE WHEN ");
            BuildExpression(methodCall.Arguments[0]);
            Append(" > ");
            BuildExpression(methodCall.Arguments[1]);
            Append(" THEN ");
            BuildExpression(methodCall.Arguments[0]);
            Append(" ELSE  ");
            BuildExpression(methodCall.Arguments[1]);
            Append(" END ");
        }

        /// <summary>
        /// 最小值比较
        /// </summary>
        /// <param name="methodCall"></param>
        protected virtual void BuildMathMin(MethodCallExpression methodCall)
        {
            Append("CASE WHEN ");
            BuildExpression(methodCall.Arguments[0]);
            Append(" < ");
            BuildExpression(methodCall.Arguments[1]);
            Append(" THEN ");
            BuildExpression(methodCall.Arguments[0]);
            Append(" ELSE  ");
            BuildExpression(methodCall.Arguments[1]);
            Append(" END ");
        }

        protected virtual void BuildMathRound(MethodCallExpression methodCall)
        {
            Append("Round(");

            var arguments = methodCall.Arguments;
            BuildExpression(arguments[0]);

            Append(',');
            if (arguments.Count == 1)
            {
                Append('0');
            }
            else
            {
                BuildExpression(arguments[1]);
            }

            if (arguments.Count == 3)
            {
                var mode = arguments[2].Cast<ConstantExpression>().Value.Cast<MidpointRounding>();
                Append(',', (int)mode);
            }
            Append(')');
        }
    }
}
