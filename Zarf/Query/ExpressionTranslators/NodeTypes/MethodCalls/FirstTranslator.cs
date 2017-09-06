using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators.Methods
{
    public class FirstTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static FirstTranslator()
        {
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "First" || item.Name == "FirstOrDefault");
        }

        public override Expression Translate(IQueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var rootQuery = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();

            if (methodCall.Arguments.Count == 2)
            {
                var condition = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

                if (rootQuery.Sets.Count != 0)
                {
                    rootQuery = rootQuery.PushDownSubQuery(context.Alias.GetNewTable(), context.UpdateRefrenceSource);
                    rootQuery.Result = rootQuery.SubQuery.Result;
                }

                context.QuerySourceProvider.AddSource(condition.Parameters.FirstOrDefault(), rootQuery);
                rootQuery.AddWhere(transformVisitor.Visit(condition).UnWrap());
            }

            if (methodCall.Method.Name == "FirstOrDefault")
            {
                rootQuery.DefaultIfEmpty = true;
            }

            rootQuery.Limit = 1;
            return rootQuery;
        }
    }
}
