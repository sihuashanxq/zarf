using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Zarf.Infrastructure
{
    /// <summary>
    /// 表达式相等HashCode计算器
    /// 常数计算具体值的HashCode
    /// </summary>
    public class ExprssionEqualityHashCodeCalculator
    {
        public virtual int GetHashCode(Expression expression)
        {
            if (expression == null)
            {
                return 0;
            }
            var hashCode = expression.NodeType.GetHashCode();

            hashCode += (hashCode * 37) ^ expression.Type.GetHashCode();

            switch (expression)
            {
                case UnaryExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case ConstantExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case ParameterExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case MemberExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case MethodCallExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case LambdaExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case NewExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case MemberInitExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case ConditionalExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case DefaultExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case ListInitExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case InvocationExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case NewArrayExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case BinaryExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                case TypeBinaryExpression node:
                    hashCode += (hashCode * 37) ^ GetHashCode(node);
                    break;
                default:
                    if (expression.NodeType != ExpressionType.Extension)
                    {
                        throw new NotImplementedException();
                    }

                    hashCode += (hashCode * 37) ^ expression.GetHashCode();
                    break;
            }

            return hashCode;
        }

        protected virtual int GetHashCode(UnaryExpression unary)
        {
            var hashCode = unary.Method?.GetHashCode() ?? 0;

            return hashCode += (hashCode * 37) ^ GetHashCode(unary.Operand);
        }

        protected virtual int GetHashCode(ConstantExpression constant)
        {
            return constant.Value?.GetHashCode() ?? 0;
        }

        protected virtual int GetHashCode(ParameterExpression parameter)
        {
            return parameter.Name?.GetHashCode() ?? 0;
        }

        protected virtual int GetHashCode(MemberExpression member)
        {
            var hashCode = member.Member.GetHashCode();

            hashCode += (hashCode * 37) ^ GetHashCode(member.Expression);

            return hashCode;
        }

        protected virtual int GetHashCode(MethodCallExpression methodCall)
        {
            var hashCode = methodCall.Method.GetHashCode();

            hashCode += (hashCode * 37) ^ GetHashCode(methodCall.Object);
            hashCode += (hashCode * 37) ^ GetHashCode(methodCall.Arguments);

            return hashCode;
        }

        protected virtual int GetHashCode(LambdaExpression lambda)
        {
            var hashCode = lambda.ReturnType.GetHashCode();

            hashCode += (hashCode * 37) ^ GetHashCode(lambda.Body);
            hashCode += (hashCode * 37) ^ GetHashCode(lambda.Parameters);

            return hashCode;
        }

        protected virtual int GetHashCode(NewExpression newExpression)
        {
            var hashCode = newExpression.Constructor?.GetHashCode() ?? 0;

            if (newExpression.Members != null)
            {
                foreach (var item in newExpression.Members)
                {
                    hashCode += (hashCode * 37) ^ item.GetHashCode();
                }
            }

            return hashCode += (hashCode * 37) ^ GetHashCode(newExpression.Arguments);
        }

        protected virtual int GetHashCode(MemberInitExpression memberInit)
        {
            var hashCode = GetHashCode(memberInit.NewExpression);

            for (var i = 0; i < memberInit.Bindings.Count; i++)
            {
                var memberBinding = memberInit.Bindings[i];

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

            return hashCode;
        }

        protected virtual int GetHashCode(BinaryExpression binary)
        {
            var hashCode = GetHashCode(binary.Left);
            hashCode += (hashCode * 37) ^ GetHashCode(binary.Right);

            return hashCode;
        }

        protected virtual int GetHashCode(TypeBinaryExpression typeBinary)
        {
            var hashCode = GetHashCode(typeBinary.Expression);

            hashCode += (hashCode * 37) ^ typeBinary.TypeOperand.GetHashCode();

            return hashCode;
        }

        protected virtual int GetHashCode(ConditionalExpression conditional)
        {
            var hashCode = GetHashCode(conditional.Test);

            hashCode += (hashCode * 37) ^ GetHashCode(conditional.IfTrue);
            hashCode += (hashCode * 37) ^ GetHashCode(conditional.IfFalse);

            return hashCode;
        }

        protected virtual int GetHashCode(DefaultExpression defaultExpression)
        {
            return defaultExpression.GetHashCode();
        }

        protected virtual int GetHashCode(ListInitExpression listInit)
        {
            var hashCode = GetHashCode(listInit.NewExpression);

            for (var i = 0; i < listInit.Initializers.Count; i++)
            {
                hashCode += (hashCode * 37) ^ GetHashCode(listInit.Initializers[i].Arguments);
            }

            return hashCode;
        }

        protected virtual int GetHashCode(InvocationExpression invocation)
        {
            var hashCode = GetHashCode(invocation.Expression);

            return hashCode += (hashCode * 37) ^ GetHashCode(invocation.Arguments);
        }

        protected virtual int GetHashCode(NewArrayExpression newArray)
        {
            return GetHashCode(newArray.Expressions);
        }

        protected virtual int GetHashCode(IEnumerable<Expression> expressions)
        {
            var hashCode = 0;

            foreach (var item in expressions)
            {
                hashCode += (hashCode * 37) ^ GetHashCode(item);
            }

            return hashCode;
        }
    }
}
