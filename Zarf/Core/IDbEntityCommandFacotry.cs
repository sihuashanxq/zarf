namespace Zarf.Core
{
    public interface IDbEntityCommandFacotry
    {
        IDbEntityCommand Create(IDbEntityConnection entityConnection);

        IDbEntityCommand Create();
    }
}
