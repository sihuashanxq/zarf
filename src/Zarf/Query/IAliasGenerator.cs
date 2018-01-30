namespace Zarf
{
    public interface IAliasGenerator
    {
        string GetNewTable();

        string GetNewColumn();

        string GetNewParameter();

        void Reset();
    }
}
