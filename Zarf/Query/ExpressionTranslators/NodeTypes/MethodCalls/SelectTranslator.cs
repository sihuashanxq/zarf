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
            SupprotedMethods = ReflectionUtil.AllQueryableMethods.Where(item => item.Name == "Select");
        }

        public override Expression Translate(QueryContext context, MethodCallExpression methodCall, ExpressionVisitor transformVisitor)
        {
            var query = transformVisitor.Visit(methodCall.Arguments[0]).As<QueryExpression>();
            var selector = methodCall.Arguments[1].UnWrap().As<LambdaExpression>();

            if (query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(context.CreateAlias(), context.UpdateRefrenceSource);
            }

            query.Projections.Clear();

            context.Projections = new List<Expression>();
            context.QuerySource[selector.Parameters.First()] = query;
            var entityNew = transformVisitor.Visit(selector).UnWrap();

            foreach (var item in context.Projections)
            {
                if (!item.Is<FromTableExpression>())
                {
                    query.Projections.Add(item);
                    continue;
                }

                query.Projections.AddRange(item.As<FromTableExpression>().GenerateColumns());
            }

            query.Result = new EntityResult(entityNew, methodCall.Method.ReturnType.GetElementTypeInfo());
            return query;
        }
    }
}