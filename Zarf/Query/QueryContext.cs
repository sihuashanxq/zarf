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
        public IMemberAccessMapper MemberAccessMapper { get; }

        public IQueryColumnOrdinalMapper ProjectionMappingProvider { get; }

        public IPropertyNavigationContext PropertyNavigationContext { get; }

        public ILambdaParameterMapper LambdaParameterMapper { get; }

        public IProjectionScanner ProjectionScanner { get; }

        public IAliasGenerator Alias { get; }

        public IMemberValueCache MemberValueCache { get; }

        public IDbContextParts DbContextParts { get; }

        public IQueryColumnCaching ColumnCaching { get; }

        public QueryContext(
            IMemberAccessMapper memberMappingProvider,
            IQueryColumnOrdinalMapper projectionMappingProvider,
            IPropertyNavigationContext navigationContext,
            ILambdaParameterMapper sourceProvider,
            IProjectionScanner projectionFinder,
            IAliasGenerator aliasGenerator,
            IMemberValueCache memValueCache,
            IDbContextParts dbContextParts
            )
        {
            MemberAccessMapper = memberMappingProvider;
            ProjectionMappingProvider = projectionMappingProvider;
            PropertyNavigationContext = navigationContext;
            LambdaParameterMapper = sourceProvider;
            ProjectionScanner = projectionFinder;
            Alias = aliasGenerator;
            MemberValueCache = memValueCache;
            DbContextParts = dbContextParts;
            ColumnCaching = new QueryColumnCaching();
        }
    }
}
