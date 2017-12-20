using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query.Expressions
{
    /// <summary>
    /// 表达式比较
    /// </summary>
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        public int GetHashCode(Expression obj)
        {
            if (obj == null)
            {
                return 0;
            }

            unchecked
            {
                var hashCode = (int)obj.NodeType;

                hashCode += (hashCode * 37) ^ obj.Type.GetHashCode();

                switch (obj.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                    case ExpressionType.UnaryPlus:
                        {
                            var unaryExpression = (UnaryExpression)obj;

                            if (unaryExpression.Method != null)
                            {
                                hashCode += hashCode * 37 ^ unaryExpression.Method.GetHashCode();
                            }

                            hashCode += (hashCode * 37) ^ GetHashCode(unaryExpression.Operand);

                            break;
                        }
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.Power:
                        {
                            var binaryExpression = (BinaryExpression)obj;

                            hashCode += (hashCode * 37) ^ GetHashCode(binaryExpression.Left);
                            hashCode += (hashCode * 37) ^ GetHashCode(binaryExpression.Right);

                            break;
                        }
                    case ExpressionType.TypeIs:
                        {
                            var typeBinaryExpression = (TypeBinaryExpression)obj;

                            hashCode += (hashCode * 37) ^ GetHashCode(typeBinaryExpression.Expression);
                            hashCode += (hashCode * 37) ^ typeBinaryExpression.TypeOperand.GetHashCode();

                            break;
                        }
                    case ExpressionType.Constant:
                        {
                            var constantExpression = (ConstantExpression)obj;
                            if (constantExpression.Value != null)
                            {
                                hashCode += (hashCode * 37) ^ constantExpression.Value.GetHashCode();
                            }

                            break;
                        }
                    case ExpressionType.Parameter:
                        {
                            var parameterExpression = (ParameterExpression)obj;

                            hashCode += hashCode * 37;
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            if (parameterExpression.Name != null)
                            {
                                hashCode ^= parameterExpression.Name.GetHashCode();
                            }

                            break;
                        }
                    case ExpressionType.MemberAccess:
                        {
                            var memberExpression = (MemberExpression)obj;

                            hashCode += (hashCode * 37) ^ memberExpression.Member.GetHashCode();
                            hashCode += (hashCode * 37) ^ GetHashCode(memberExpression.Expression);

                            break;
                        }
                    case ExpressionType.Call:
                        {
                            var methodCallExpression = (MethodCallExpression)obj;

                            hashCode += (hashCode * 37) ^ methodCallExpression.Method.GetHashCode();
                            hashCode += (hashCode * 37) ^ GetHashCode(methodCallExpression.Object);
                            hashCode += (hashCode * 37) ^ GetHashCode(methodCallExpression.Arguments);

                            break;
                        }
                    case ExpressionType.Lambda:
                        {
                            var lambdaExpression = (LambdaExpression)obj;

                            hashCode += (hashCode * 37) ^ lambdaExpression.ReturnType.GetHashCode();
                            hashCode += (hashCode * 37) ^ GetHashCode(lambdaExpression.Body);
                            hashCode += (hashCode * 37) ^ GetHashCode(lambdaExpression.Parameters);

                            break;
                        }
                    case ExpressionType.New:
                        {
                            var newExpression = (NewExpression)obj;

                            hashCode += (hashCode * 37) ^ (newExpression.Constructor?.GetHashCode() ?? 0);

                            if (newExpression.Members != null)
                            {
                                for (var i = 0; i < newExpression.Members.Count; i++)
                                {
                                    hashCode += (hashCode * 37) ^ newExpression.Members[i].GetHashCode();
                                }
                            }

                            hashCode += (hashCode * 37) ^ GetHashCode(newExpression.Arguments);

                            break;
                        }
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                        {
                            var newArrayExpression = (NewArrayExpression)obj;

                            hashCode += (hashCode * 37) ^ GetHashCode(newArrayExpression.Expressions);

                            break;
                        }
                    case ExpressionType.Invoke:
                        {
                            var invocationExpression = (InvocationExpression)obj;

                            hashCode += (hashCode * 37) ^ GetHashCode(invocationExpression.Expression);
                            hashCode += (hashCode * 37) ^ GetHashCode(invocationExpression.Arguments);

                            break;
                        }
                    case ExpressionType.MemberInit:
                        {
                            var memberInitExpression = (MemberInitExpression)obj;

                            hashCode += (hashCode * 37) ^ GetHashCode(memberInitExpression.NewExpression);

                            for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                            {
                                var memberBinding = memberInitExpression.Bindings[i];

                                hashCode += (hashCode * 37) ^ memberBinding.Member.GetHashCode();
                                hashCode += (hashCode * 37) ^ (int)memberBinding.BindingType;

                                switch (memberBinding.BindingType)
                                {
                                    case MemberBindingType.Assignment:
                                        var memberAssignment = (MemberAssignment)memberBinding;
                                        hashCode += (hashCode * 37) ^ GetHashCode(memberAssignment.Expression);
                                        break;
                                    case MemberBindingType.ListBinding:
                                        var memberListBinding = (MemberListBinding)memberBinding;
                                        for (var j = 0; j < memberListBinding.Initializers.Count; j++)
                                        {
                                            hashCode += (hashCode * 37) ^ GetHashCode(memberListBinding.Initializers[j].Arguments);
                                        }
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }

                            break;
                        }
                    case ExpressionType.ListInit:
                        {
                            var listInitExpression = (ListInitExpression)obj;

                            hashCode += (hashCode * 37) ^ GetHashCode(listInitExpression.NewExpression);

                            for (var i = 0; i < listInitExpression.Initializers.Count; i++)
                            {
                                hashCode += (hashCode * 37) ^ GetHashCode(listInitExpression.Initializers[i].Arguments);
                            }

                            break;
                        }
                    case ExpressionType.Conditional:
                        {
                            var conditionalExpression = (ConditionalExpression)obj;

                            hashCode += (hashCode * 37) ^ GetHashCode(conditionalExpression.Test);
                            hashCode += (hashCode * 37) ^ GetHashCode(conditionalExpression.IfTrue);
                            hashCode += (hashCode * 37) ^ GetHashCode(conditionalExpression.IfFalse);

                            break;
                        }
                    case ExpressionType.Default:
                        {
                            hashCode += (hashCode * 37) ^ obj.Type.GetHashCode();
                            break;
                        }
                    case ExpressionType.Extension:
                        {
                            hashCode += (hashCode * 37) ^ obj.GetHashCode();
                            break;
                        }
                }

                return hashCode;
            }
        }

        private int GetHashCode<T>(IList<T> expressions)
            where T : Expression
        {
            var hashCode = 0;

            for (var i = 0; i < expressions.Count; i++)
            {
                hashCode += (hashCode * 37) ^ GetHashCode(expressions[i]);
            }

            return hashCode;
        }

        public bool Equals(Expression x, Expression y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }
    }
}