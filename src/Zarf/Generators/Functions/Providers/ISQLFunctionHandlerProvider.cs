using System;
using System.Collections.Generic;
using System.Reflection;

namespace Zarf.Generators.Functions.Providers
{
    /// <summary>
    /// SQL函数处理器提供者
    /// </summary>
    public interface ISQLFunctionHandlerProvider
    {
        IEnumerable<ISQLFunctionHandler> GetHandlers(MethodInfo method);
    }
}
