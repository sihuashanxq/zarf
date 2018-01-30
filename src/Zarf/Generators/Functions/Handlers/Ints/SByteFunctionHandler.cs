using System;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class SByteFunctionHandler : ByteFunctionHandler
    {
        public override Type SoupportedType => typeof(sbyte);
    }

    public class SByteNullFunctionHandler : ByteFunctionHandler
    {
        public override Type SoupportedType => typeof(sbyte?);
    }
}
