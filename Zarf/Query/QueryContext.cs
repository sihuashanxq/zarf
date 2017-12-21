using System.Reflection;
using System.Collections.Generic;

using Zarf.Mapping;
using Zarf.Query.Expressions;
using Zarf.Core;
using System.Linq.Expressions;
using Zarf.Query.Internals;

namespace Zarf.Query
{
    public interface IMemberValueCache
    {
        void SetValue(MemberInfo mem, object value);

        object GetValue(MemberInfo mem);
    }

    public class MemberValueCache : IMemberValueCache
    {
        private Dictionary<MemberInfo, object> _memValues = new Dictionary<MemberInfo, object>();

        public object GetValue(MemberInfo mem)
        {
            if (_memValues.TryGetValue(mem, out object value))
            {
                return value;
            }

            return null;
        }

        public void SetValue(MemberInfo mem, object value)
        {
            _memValues[mem] = value;
        }
    }

    public class QueryContext : IQueryContext
    {
        public IQueryColumnOrdinalMapper ProjectionMappingProvider { get; }

        public IPropertyNavigationContext PropertyNavigationContext { get; }

        public IQueryMapper QueryMapper { get; }

        public IAliasGenerator Alias { get; }

        public IMemberValueCache MemberValueCache { get; }

        public IDbContextParts DbContextParts { get; }

        public IQueryColumnCaching ColumnCaching { get; }

        public MemberBindingMapper MemberBindingMapper { get; }

        public ProjectionOwnerMapper ProjectionOwner { get; }

        public QueryModelMapper QueryModelMapper { get; }

        public QueryContext(
            IQueryColumnOrdinalMapper projectionMappingProvider,
            IPropertyNavigationContext navigationContext,
            IQueryMapper sourceProvider,
            IAliasGenerator aliasGenerator,
            IMemberValueCache memValueCache,
            IDbContextParts dbContextParts
            )
        {
            ProjectionMappingProvider = projectionMappingProvider;
            PropertyNavigationContext = navigationContext;
            QueryMapper = sourceProvider;
            Alias = aliasGenerator;
            MemberValueCache = memValueCache;
            DbContextParts = dbContextParts;
            ColumnCaching = new QueryColumnCaching();
            MemberBindingMapper = new MemberBindingMapper();
            ProjectionOwner = new ProjectionOwnerMapper();
            QueryModelMapper = new QueryModelMapper();
        }
    }
}
