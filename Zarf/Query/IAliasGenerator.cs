namespace Zarf
{
    public interface IAliasGenerator
    {
        string GetNewTableAlias();

        string GetNewColumnAlias();

        void Reset();
    }
}
