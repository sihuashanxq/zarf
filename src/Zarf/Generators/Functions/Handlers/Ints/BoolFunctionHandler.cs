using System;

namespace Zarf.Generators.Functions.Handlers.Ints
{
    public class BoolFunctionHandler : ByteFunctionHandler
    {
        public override Type SoupportedType => typeof(bool);
    }

    public class BoolNullFunctionHandler : BoolFunctionHandler
    {
        public override Type SoupportedType => typeof(bool?);
    }
}
