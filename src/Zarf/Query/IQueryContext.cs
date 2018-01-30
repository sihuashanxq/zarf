using System.Linq.Expressions;
using Zarf.Query.Expressions;
using Zarf.Query.Mappers;

namespace Zarf.Query
{
    public interface IQueryContext
    {
        IAliasGenerator AliasGenerator { get; }

        IQueryValueCache QueryValueCache { get; }

        IMapper<Expression, SelectExpression> SelectMapper { get; }

        IMapper<Expression, QueryEntityModel> ModelMapper { get; }

        IMapper<Expression, Expression> ExpressionMapper { get; }

        IMapper<MemberExpression, Expression> BindingMaper { get; }

        DbContext DbContext { get; }
    }
}
