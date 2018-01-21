using System.Collections.Generic;
using Zarf.Query.Expressions;
using System.Linq.Expressions;
using Zarf.Query.Internals;


namespace Zarf.Query
{
    public interface IQueryContext
    {
        IQueryMapper QueryMapper { get; }

        IAliasGenerator Alias { get; }

        ISubQueryValueCache MemberValueCache { get; }

        IQueryProjectionMapper ColumnCaching { get; }

        MemberBindingMapper MemberBindingMapper { get; }

        ProjectionOwnerMapper ProjectionOwner { get; }

        QueryModelMapper QueryModelMapper { get; }

        IQueryProjectionMapper ExpressionMapper { get; }
    }

    public class ProjectionOwnerMapper
    {
        protected Dictionary<Expression, SelectExpression> Selects = new Dictionary<Expression, SelectExpression>();

        public void AddSelectProjection(Expression projection, SelectExpression select)
        {
            Selects[projection] = select;
        }

        public SelectExpression GetSelectExpression(Expression projection)
        {
            return Selects.TryGetValue(projection, out var container)
                ? container
                : default(SelectExpression);
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
            if (mem == null)
            {
                return null;
            }

            return MemberBindings.TryGetValue(mem, out var mappedExpression)
                ? mappedExpression
                : default(Expression);
        }

        public void Map(MemberExpression mem, Expression mapped)
        {
            if (mem != null)
            {
                MemberBindings[mem] = mapped;
            }
        }
    }

    public class QueryModelMapper
    {
        protected Dictionary<Expression, QueryEntityModel> QueryModeles { get; } = new Dictionary<Expression, QueryEntityModel>();

        public void MapQueryModel(Expression exp, QueryEntityModel queryModel)
        {
            if (queryModel == null)
            {
                return;
            }

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
