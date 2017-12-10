using System.Reflection;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public class QueryColumnCacheKey
    {
        public QueryExpression Query { get; }

        public MemberInfo Member { get; }

        public QueryColumnCacheKey(QueryExpression query, MemberInfo member)
        {
            Query = query;
            Member = member;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Query?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Member?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return GetHashCode() == (obj as QueryColumnCacheKey).GetHashCode();
        }

        public static bool operator ==(QueryColumnCacheKey left, QueryColumnCacheKey right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(QueryColumnCacheKey left, QueryColumnCacheKey right)
        {
            return !(left == right);
        }
    }
}
