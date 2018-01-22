using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;
using Zarf.Query.ExpressionVisitors;

namespace Zarf.Query.ExpressionTranslators.NodeTypes.MethodCalls
{
    public class WhereTranslator : MethodTranslator
    {
        public static IEnumerable<MethodInfo> SupprotedMethods { get; }

        static WhereTranslator()
        {
            SupprotedMethods = ReflectionUtil.QueryableMethods.Where(item => item.Name == "Where")
                ;
        }

        public WhereTranslator(IQueryContext queryContext, IQueryCompiler queryCompiper) : base(queryContext, queryCompiper)
        {

        }

        public override SelectExpression Translate(SelectExpression select, Expression predicate, MethodInfo method)
        {
            if (predicate == null)
            {
                return select;
            }

            var parameter = predicate.GetParameters().FirstOrDefault();

            Utils.CheckNull(select, "query");

            QueryContext.SelectMapper.Map(parameter, select);
            QueryContext.ModelMapper.Map(parameter, select.QueryModel);

            predicate = CreateRealtionCompiler(select).Compile(predicate);
            predicate = new RelationExpressionVisitor().Visit(predicate);

            predicate = new SubQueryModelRewriter(select, QueryContext).ChangeQueryModel(predicate);

            select.DefaultIfEmpty = method.Name.Contains("Default");
            select.CombineCondtion(predicate);
            select.QueryModel.ModelType = method.ReturnType;

            return select;
        }

        protected RelationExpressionCompiler CreateRealtionCompiler(SelectExpression select)
        {
            return new RelationExpressionCompiler(QueryContext);
        }
    }
}