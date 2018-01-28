namespace Zarf.Generators.Functions.Registrars
{
    public interface ISQLFunctionHandlerRegistrar
    {
        void Register(ISQLFunctionHandler handler);
    }
}
