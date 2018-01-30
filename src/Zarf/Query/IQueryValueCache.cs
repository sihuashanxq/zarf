namespace Zarf.Query
{
    public interface IQueryValueCache
    {
        void SetValue(QueryEntityModel queryModel, object value);

        object GetValue(QueryEntityModel queryModel);
    }
}
