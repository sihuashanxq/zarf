namespace Zarf.Query.Internals
{
    public class QueryContextFacotry : IQueryContextFactory
    {
        public QueryContextFacotry()
        {

        }

        public IQueryContext CreateContext()
        {
            return new QueryContext(
                    new QueryMapper(),
                    new AliasGenerator(),
                    new SubQueryValueCache()
                );
        }
    }
}
