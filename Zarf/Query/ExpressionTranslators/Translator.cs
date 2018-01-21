using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators
{
    public abstract class Translator<TExpression> : ITranslator<TExpression>, ITranslaor
    {
        public IQueryContext QueryContext { get; }

        public IQueryCompiler QueryCompiler { get; }

        public Translator(IQueryContext queryContext, IQueryCompiler queryCompiper)
        {
            QueryContext = queryContext;
            QueryCompiler = queryCompiper;
        }

        public abstract Expression Translate(TExpression expression);

        public virtual SelectExpression Translate(SelectExpression select, Expression expression, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public Expression Translate(Expression expression)
        {
            return Translate(expression.Cast<TExpression>());
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
