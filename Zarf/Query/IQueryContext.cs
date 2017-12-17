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
        IMemberAccessMapper MemberAccessMapper { get; }

        ILambdaParameterMapper LambdaParameterMapper { get; }

        IProjectionScanner ProjectionScanner { get; }

        IPropertyNavigationContext PropertyNavigationContext { get; }

        IQueryColumnOrdinalMapper ProjectionMappingProvider { get; }

        IAliasGenerator Alias { get; }

        IMemberValueCache MemberValueCache { get; }

        IDbContextParts DbContextParts { get; }

        IQueryColumnCaching ColumnCaching { get; }

        MemberBindingMapper MemberBindingMapper { get; }

        ProjectionContainerMapper Container { get; }

        QueryModelMapper QueryModelMapper { get; }
    }

    public class ProjectionContainerMapper
    {
        protected Dictionary<Expression, QueryExpression> Containers = new Dictionary<Expression, QueryExpression>();

        public void AddProjection(Expression projection, QueryExpression container)
        {
            Containers[projection] = container;
        }

        public QueryExpression GetContainer(Expression projection)
        {
            return Containers.TryGetValue(projection, out var container)
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
            return MemberBindings.TryGetValue(mem, out var mappedExpression)
                ? mappedExpression
                : default(Expression);
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
            return QueryModeles.TryGetValue(exp, out var model)
                ? model
                : default(QueryEntityModel);
        }
    }
}
