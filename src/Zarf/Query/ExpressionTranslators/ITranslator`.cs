﻿using System.Linq.Expressions;

namespace Zarf.Query.ExpressionTranslators
{
    public interface ITranslator<in T> : ITranslaor
    {
        Expression Translate( T query);
    }

    public interface ITranslaor
    {
        Expression Translate(Expression query);
    }
}