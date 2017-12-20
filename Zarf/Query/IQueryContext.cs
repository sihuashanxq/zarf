using Zarf.Mapping;
using System.Collections.Generic;
using Zarf.Query.Expressions;
using Zarf.Core;
using System.Linq.Expressions;
using Zarf.Query.Internals;
using Zarf.Entities;

namespace Zarf.Query
{
    public interface IQueryContext
    {
        ILambdaParameterMapper ParameterQueryMapper { get; }

        IPropertyNavigationContext PropertyNavigationContext { get; }

        IQueryColumnOrdinalMapper ProjectionMappingProvider { get; }

        IAliasGenerator Alias { get; }

        IMemberValueCache MemberValueCache { get; }

        IDbContextParts DbContextParts { get; }

        IQueryColumnCaching ColumnCaching { get; }

        MemberBindingMapper MemberBindingMapper { get; }

        ProjectionOwnerMapper ProjectionOwner { get; }

        QueryModelMapper QueryModelMapper { get; }
    }

    public class ProjectionOwnerMapper
    {
        protected Dictionary<Expression, QueryExpression> Queries = new Dictionary<Expression, QueryExpression>();

        public void AddProjection(Expression projection, QueryExpression container)
        {
            Queries[projection] = container;
        }

        public QueryExpression GetQuery(Expression projection)
        {
            return Queries.TryGetValue(projection, out var container)
                ? container
                : default(QueryExpression);
        }
    }

    public class MemberBindingMapper
    {
        protected Dictionary<MemberExpression, Expression> MemberBindings { get; }

        public MemberBindingMapper()
        {
            MemberBindings = new Dictionary<MemberExpression, Expression>(new ExpressionEqualityComparer());
        }

        public Expression GetMapedExpression(MemberExpression mem)
        {
            foreach (var item in MemberBindings)
            {
                var x = new ExpressionEqualityComparer().GetHashCode(item.Key);
                var y = new ExpressionEqualityComparer().GetHashCode(mem);
            }

            var b = MemberBindings.TryGetValue(mem, out var mappedExpression)
                ? mappedExpression
                : default(Expression);
            return b;
        }

        public void Map(MemberExpression mem, Expression mapped)
        {
            MemberBindings[mem] = mapped;
        }
    }

    public class QueryModelMapper
    {
        protected Dictionary<Expression, QueryEntityModel> QueryModeles { get; } = new Dictionary<Expression, QueryEntityModel>();

        public void MapQueryModel(Expression exp, QueryEntityModel queryModel)
        {
            QueryModeles[exp] = queryModel;
        }

        public QueryEntityModel GetQueryModel(Expression exp)
        {
            return exp != null && QueryModeles.TryGetValue(exp, out var model)
                ? model
                : default(QueryEntityModel);
        }
    }
}
