namespace Zarf
{
    public interface IAliasGenerator
    {
        string GetNewTable();

        string GetNewColumn();

        void Reset();
    }
}
