using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    /// <summary>
    /// Select Query
    /// </summary>
    public class SelectTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static SelectTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Select")
                .Concat(new[] { ReflectionUtil.JoinSelect });
        }

        public SelectTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            var methodBody = methodCall.Method.GetGenericMethodDefinition();
            if (query.Sets.Count != 0 || query.Columns.Count != 0)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            if (methodBody == ReflectionUtil.JoinSelect)
            {
                RegisterJoinSelectQueries(query, methodCall.Arguments[1]);
            }
            else
            {
                MapParameterWithQuery(GetFirstParameter(methodCall.Arguments[1]), query);
            }

            var template = GetCompiledExpression(methodCall.Arguments[1]).UnWrap();
            var tem = new ResultExpressionVisitor(query).Visit(template);
            query.AddColumns(GetColumns(tem));
            query.Result = new EntityResult(tem, methodCall.Method.ReturnType.GetCollectionElementType());
            query.ChangeTypeOfExpression(query.Result.ElementType);
            return query;
        }

        protected virtual void RegisterJoinSelectQueries(QueryExpression query, Expression selector)
        {
            var parameters = GetParameteres(selector);
            var i = 0;
            while (i < parameters.Count)
            {
                if (i == 0)
                    MapParameterWithQuery(parameters[i++], query);
                else
                    MapParameterWithQuery(parameters[i++], query.Joins[i - 1].Query);
            }
        }
    }

    public class ResultExpressionVisitor : ExpressionVisitors.ExpressionVisitorBase
    {
        public QueryExpression Root { get; }

        public ResultExpressionVisitor(QueryExpression root)
        {
            Root = root;
        }

        public override Expression Visit(Expression node)
        {
            if (node.Is<QueryExpression>())
            {
                var q = node.As<QueryExpression>();
                Root.AddJoin(new JoinExpression(node.As<QueryExpression>(), null, JoinType.Cross));
                q.Limit = 1;
                if (q.Columns.Count == 0)
                {
                    foreach (var item in q.GenerateTableColumns())
                    {
                        q.AddColumns(new[] { new ColumnDescriptor() {
                            Member=item.As<ColumnExpression>()?.Member,
                            Expression=item
                        }});
                    }
                }

                return node;
            }
            else if (node.NodeType == ExpressionType.Extension)
            {
                return node;
            }

            return base.Visit(node);

            if (node.Is<AllExpression>())
            {

            }

            if (node.Is<AnyExpression>())
            {

            }

            return base.Visit(node);
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            var lambdaBody = Visit(lambda.Body);
            if (lambdaBody != lambda.Body)
            {
                return Expression.Lambda(lambdaBody, lambda.Parameters);
            }

            return lambda;
        }
    }
}