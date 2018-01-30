using System;
using System.Linq.Expressions;
using System.Collections.Generic;
namespace Zarf.Generators.Functions
{
    /// <summary>
    /// SQL函数处理器
    /// </summary>
    public interface ISQLFunctionHandler
    {
        Type SoupportedType { get; }

        bool HandleFunction(ISQLGenerator generator, MethodCallExpression methodCall);
    }
}
