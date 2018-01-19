using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Queries.Expressions;
using Zarf.Queries.ExpressionVisitors;

namespace Zarf.Queries.ExpressionTranslators
{
    public abstract class Translator<TExpression> : ITranslator<TExpression>, ITranslaor
    {
        public IQueryContext Context { get; }

        public IQueryCompiler Compiler { get; }

        public Translator(IQueryContext queryContext, IQueryCompiler queryCompiper)
        {
            Context = queryContext;
            Compiler = queryCompiper;
        }

        public abstract Expression Translate(TExpression query);

        public Expression Translate(Expression query) => Translate(query.Cast<TExpression>());

        protected TNodeType GetCompiledExpression<TNodeType>(Expression exp)
            where TNodeType : Expression
        {
            return Compiler.Compile(exp) as TNodeType;
        }

        protected Expression GetCompiledExpression(Expression exp)
        {
            return GetCompiledExpression<Expression>(exp);
        }

        protected List<ParameterExpression> GetParameteres(Expression lambda)
        {
            return lambda.UnWrap().As<LambdaExpression>().Parameters.ToList();
        }
    }
}
