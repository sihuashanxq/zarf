using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.Handlers
{
    public abstract class QueryNodeHandler<TExpression> : IQueryNodeHandler<TExpression>, IQueryNodeHandler
    {
        public IQueryContext QueryContext { get; }

        public IQueryCompiler QueryCompiler { get; }

        public QueryNodeHandler(IQueryContext queryContext, IQueryCompiler queryCompiper)
        {
            QueryContext = queryContext;
            QueryCompiler = queryCompiper;
        }

        public abstract Expression HandleNode(TExpression expression);

        public virtual SelectExpression HandleNode(SelectExpression select, Expression expression, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public Expression HandleNode(Expression expression)
        {
            return HandleNode(expression.Cast<TExpression>());
        }

        protected TNodeType Compile<TNodeType>(Expression expression) where TNodeType : Expression
        {
            return QueryCompiler.Compile(expression) as TNodeType;
        }

        protected Expression Compile(Expression expression)
        {
            return Compile<Expression>(expression);
        }
    }
}
