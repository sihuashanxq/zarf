using System;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class UShortFunctionHandler : IntFunctionHandler
    {
        public override Type SoupportedType => typeof(UInt16);
    }

    public class UShortNullFunctionHandler : UShortFunctionHandler
    {
        public override Type SoupportedType => typeof(UInt16?);
    }
}
