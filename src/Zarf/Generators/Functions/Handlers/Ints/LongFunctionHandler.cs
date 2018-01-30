using System;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class LongFunctionHandler : UIntFunctionHandler
    {
        public override Type SoupportedType => typeof(Int64);
    }

    public class LongNullFunctionHandler : LongFunctionHandler
    {
        public override Type SoupportedType => typeof(Int64?);
    }
}
