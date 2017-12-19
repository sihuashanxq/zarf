using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query.Expressions
{
    /// <summary>
    /// 表达式比较
    /// </summary>
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        public bool Equals(Expression x, Expression y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(Expression expression)
        {
            return new HashCodeExpressionVisitor(expression).HashCode;
        }

        private class HashCodeExpressionVisitor : ExpressionVisitor
        {
            public int HashCode { get; protected set; }

            public HashCodeExpressionVisitor(Expression expression)
            {
                HashCode = 0;
                Visit(expression);
            }

            protected override Expression VisitUnary(UnaryExpression unary)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ (unary.Method?.GetHashCode() ?? 0);
                    Visit(unary.Operand);
                    return unary;
                }
            }

            protected override Expression VisitConstant(ConstantExpression constant)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ (constant.Value?.GetHashCode() ?? 0);
                    return constant;
                }
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ (parameter?.Name.GetHashCode() ?? 0);
                    return parameter;
                }
            }

            protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinary)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ typeBinary.TypeOperand.GetHashCode();
                    Visit(typeBinary.Expression);
                    return typeBinary;
                }
            }

            protected override Expression VisitMember(MemberExpression mem)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ mem.Member.GetHashCode();
                    Visit(mem.Expression);
                    return mem;
                }
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ methodCall.Method.GetHashCode();
                    Visit(methodCall.Arguments);
                    Visit(methodCall.Object);
                    return methodCall;
                }
            }

            protected virtual Expression VisitLambda(LambdaExpression lambda)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ lambda.ReturnType.GetHashCode();

                    foreach (var item in lambda.Parameters)
                    {
                        Visit(item);
                    }

                    Visit(lambda.Body);
                    return lambda;
                }
            }

            protected override Expression VisitNew(NewExpression newExpression)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ (newExpression.Constructor?.GetHashCode() ?? 0);

                    VisitMembers(newExpression.Members);
                    Visit(newExpression.Arguments);
                    return newExpression;
                }
            }

            protected override Expression VisitNewArray(NewArrayExpression newArray)
            {
                Visit(newArray.Expressions);
                return newArray;
            }

            protected override Expression VisitInvocation(InvocationExpression invoke)
            {
                Visit(invoke.Expression);
                Visit(invoke.Arguments);
                return invoke;
            }

            protected override Expression VisitMemberInit(MemberInitExpression memberInit)
            {
                unchecked
                {
                    Visit(memberInit.NewExpression);

                    foreach (var binding in memberInit.Bindings)
                    {
                        HashCode += (HashCode * 37) ^ binding.Member.GetHashCode();
                        HashCode += (HashCode * 37) ^ binding.BindingType.GetHashCode();

                        if (binding.BindingType == MemberBindingType.ListBinding)
                        {
                            foreach (var item in (binding as MemberListBinding).Initializers)
                            {
                                HashCode += (HashCode * 37) ^ (item.AddMethod?.GetHashCode() ?? 0);
                                Visit(item.Arguments);
                            }
                        }
                        else if (binding.BindingType == MemberBindingType.Assignment)
                        {
                            Visit((binding as MemberAssignment).Expression);
                        }
                    }

                    return memberInit;
                }
            }

            protected override Expression VisitListInit(ListInitExpression listInit)
            {
                unchecked
                {
                    Visit(listInit.NewExpression);

                    foreach (var item in listInit.Initializers)
                    {
                        HashCode += (HashCode * 37) ^ (item.AddMethod?.GetHashCode() ?? 0);
                        Visit(item.Arguments);
                    }

                    return listInit;
                }
            }

            protected override Expression VisitConditional(ConditionalExpression condtional)
            {
                Visit(condtional.IfTrue);
                Visit(condtional.IfFalse);
                Visit(condtional.Test);
                return condtional;
            }

            protected override Expression VisitExtension(Expression extension)
            {
                unchecked
                {
                    HashCode += (HashCode * 37) ^ extension.GetHashCode();
                    return extension;
                }
            }

            protected override Expression VisitBinary(BinaryExpression binary)
            {
                Visit(binary.Left);
                Visit(binary.Right);
                return binary;
            }

            public override Expression Visit(Expression node)
            {
                unchecked
                {
                    if (node.NodeType != ExpressionType.Lambda)
                    {
                        base.Visit(node);
                    }
                    else
                    {
                        VisitLambda(node as LambdaExpression);
                    }

                    HashCode += (HashCode * 37) ^ node.NodeType.GetHashCode();
                    HashCode += (HashCode * 37) ^ node.Type.GetHashCode();

                    return node;
                }
            }

            protected virtual IEnumerable<MemberInfo> VisitMembers(IEnumerable<MemberInfo> members)
            {
                unchecked
                {
                    if (members == null)
                    {
                        return members;
                    }

                    foreach (var item in members)
                    {

                        HashCode += (HashCode * 37) ^ item.GetHashCode();
                    }

                    return members;
                }
            }
        }
    }
}