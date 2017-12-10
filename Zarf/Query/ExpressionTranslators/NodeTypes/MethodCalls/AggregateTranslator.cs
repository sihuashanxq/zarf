﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators.Methods;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class AggregateTranslator : Translator<MethodCallExpression>
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static AggregateTranslator()
        {
            var methods = new[] { "Max", "Sum", "Min", "Average", "Count", "LongCount" };
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => methods.Contains(item.Name));
        }

        public AggregateTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override Expression Translate(MethodCallExpression methodCall)
        {
            var query = GetCompiledExpression<QueryExpression>(methodCall.Arguments[0]);
            ColumnExpression column = null;
            AggregateExpression aggregate = null;

            if (query.Columns.Count != 0 || query.Sets.Count != 0)
            {
                query = query.PushDownSubQuery(Context.Alias.GetNewTable());
            }

            if (methodCall.Arguments.Count == 2)
            {
                MapParameterWithQuery(GetFirstParameter(methodCall.Arguments[1]), query);
                var keySelector = GetCompiledExpression(methodCall.Arguments[1]);
                var keySelectorExp = GetColumns(keySelector).FirstOrDefault()?.Expression;
                if (keySelectorExp == null)
                {
                    keySelectorExp = keySelector.UnWrap().As<LambdaExpression>().Body;
                }

                if (keySelectorExp.Is<QueryExpression>())
                {
                    throw new Exception("Cannot perform an aggregate function on an expression containing an aggregate or a subquery.");
                }

                if (keySelectorExp.Is<ConstantExpression>())
                {
                    aggregate = new AggregateExpression(methodCall.Method, keySelectorExp, query, Context.Alias.GetNewColumn());
                }
                else
                {
                    column = keySelectorExp.As<ColumnExpression>();
                    aggregate = new AggregateExpression(methodCall.Method, column, query, column.Column?.Name ?? Context.Alias.GetNewColumn());
                }
            }
            else
            {
                column = new ColumnExpression(query, new Column(Context.Alias.GetNewColumn()), methodCall.Method.ReturnType);
                aggregate = new AggregateExpression(methodCall.Method, column, query, column.Column.Name);
            }

            query.Result = new EntityResult(aggregate, methodCall.Method.ReturnType);
            query.AddColumns(new[] { new ColumnDescriptor() { Expression = aggregate, Ordinal = query.Columns.Count } });
            query.ChangeTypeOfExpression(methodCall.Method.ReturnType);
            return query;
        }
    }
}
