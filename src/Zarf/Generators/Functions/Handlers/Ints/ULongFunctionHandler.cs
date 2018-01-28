using System;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class ULongFunctionHandler : LongFunctionHandler
    {
        public override Type SoupportedType => typeof(UInt64);
    }

    public class ULongNullFunctionHandler : ULongFunctionHandler
    {
        public override Type SoupportedType => typeof(UInt64?);
    }
}
