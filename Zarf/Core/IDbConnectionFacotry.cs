namespace Zarf.Core
{
    public interface IDbConnectionFacotry
    {
        IDbConnectionWrapper Create();
    }
}
