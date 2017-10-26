using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Mapping.Bindings;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionTranslators;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query
{
    public class LinqExpressionTanslator
    {
        public Expression Build(Expression node, IQueryContext context)
        {
            var translatedExpression = new SqlTranslatingExpressionVisitor(context, NodeTypeTranslatorProvider.Default).Visit(node);
            if (translatedExpression.Is<QueryExpression>())
            {
                return translatedExpression;
            }
            else
            {
                //context.ProjectionMappingProvider.Map(translatedExpression, translatedExpression, 0);
                return translatedExpression;
            }
        }

        public Expression Translate(Expression linqExpression)
        {
            return Build(linqExpression, Context as QueryContext);
        }

        public IQueryContext Context { get; }

        public LinqExpressionTanslator(IQueryContext context)
        {
            Context = context;
        }
    }
}
