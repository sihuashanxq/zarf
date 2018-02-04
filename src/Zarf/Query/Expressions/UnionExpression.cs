namespace Zarf.Query.Expressions
{
    public class UnionExpression : SetsExpression
    {
        public bool IncludeRepated { get; }

        public UnionExpression(SelectExpression select, bool includeRepated) : base(select)
        {
            IncludeRepated = includeRepated;
        }
    }
}
