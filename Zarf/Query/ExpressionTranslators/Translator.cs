using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators
{
    public abstract class Translator<TExpression> : ITranslator<TExpression>, ITranslaor
    {
        public IQueryContext Context { get; }

        public IQueryCompiler Compiler { get; }

        public RelationExpressionVisitor RelationVisitor { get; }

        public Translator(IQueryContext queryContext, IQueryCompiler queryCompiper)
        {
            Context = queryContext;
            Compiler = queryCompiper;
            RelationVisitor = new RelationExpressionVisitor();
        }

        public abstract Expression Translate(TExpression query);

        public Expression Translate(Expression query) => Translate(query.Cast<TExpression>());

        protected void MapParameterWithQuery(ParameterExpression parameter, QueryExpression query)
        {
            Context.LambdaParameterMapper.Map(parameter, query);
        }

        protected TNodeType GetCompiledExpression<TNodeType>(Expression exp)
            where TNodeType : Expression
        {
            return Compiler.Compile(exp) as TNodeType;
        }

        protected Expression GetCompiledExpression(Expression exp)
        {
            return GetCompiledExpression<Expression>(exp);
        }

        protected List<ColumnDescriptor> GetColumns(Expression exp)
        {
            return Context.ProjectionScanner.Scan(exp);
        }

        protected List<ParameterExpression> GetParameteres(Expression lambda)
        {
            return lambda.UnWrap().As<LambdaExpression>().Parameters.ToList();
        }

        protected ParameterExpression GetFirstParameter(Expression lambda)
        {
            return lambda.UnWrap().As<LambdaExpression>().Parameters.FirstOrDefault();
        }

        protected ParameterExpression GetLastParameter(Expression lambda)
        {
            return lambda.UnWrap().As<LambdaExpression>().Parameters.LastOrDefault();
        }

        protected Expression HandleCondtion(Expression predicate)
        {
            return RelationVisitor.Visit(predicate);
        }
    }
}
