using System.Linq.Expressions;
using Zarf.Query.Expressions;
using Zarf.Query.Mappers;

namespace Zarf.Query
{
    public class QueryContext : IQueryContext
    {
        public IAliasGenerator AliasGenerator { get; }

        public IQueryValueCache QueryValueCache { get; }

        public IMapper<Expression, SelectExpression> SelectMapper { get; }

        public IMapper<Expression, QueryEntityModel> ModelMapper { get; }

        public IMapper<Expression, Expression> ExpressionMapper { get; }

        public IMapper<MemberExpression, Expression> BindingMaper { get; }

        public DbContext DbContext { get; }

        public QueryContext(
            IMapper<Expression, SelectExpression> sMapper,
            IMapper<Expression, QueryEntityModel> mMapper,
            IMapper<MemberExpression, Expression> bMapper,
            IMapper<Expression, Expression> eMapper,
            IAliasGenerator aliasGenerator,
            IQueryValueCache queryValueCache,
            DbContext dbContext
        )
        {
            BindingMaper = bMapper;
            ModelMapper = mMapper;
            SelectMapper = sMapper;
            ExpressionMapper = eMapper;
            AliasGenerator = aliasGenerator;
            QueryValueCache = queryValueCache;
            DbContext = dbContext;
        }
    }
}
