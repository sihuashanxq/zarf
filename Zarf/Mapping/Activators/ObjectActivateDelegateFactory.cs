using System;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query;
using System.Linq;
using System.Collections.Generic;

namespace Zarf.Mapping
{
    public class ObjectActivateDelegateFactory : ExpressionVisitor
    {
        private QueryContext _queryContext;

        private Expression _source;

        protected override Expression VisitExtension(Expression node)
        {
            return node;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }

            var instance = GetMemberValue(node);
            if (instance != null)
            {
                return instance;
            }

            return base.Visit(node);
        }

        private Expression GetMemberValue(Expression node)
        {
            var map = _queryContext.ProjectionMappingProvider.GetMapping(node);
            if (map != null && map.Source == _source)
            {
                var entityMemberActivator = EntityMemberActivator.CreateActivator(map);
                return Expression.Convert(
                        Expression.Call(
                            Expression.Constant(entityMemberActivator),
                            EntityMemberActivator.ActivateMethod,
                            EntityMemberActivator.ActivateMethodParameter
                          ),
                        node.Type
                       );
            }

            return null;
        }

        protected override Expression VisitMember(MemberExpression mem)
        {
            return mem;
        }

        protected override Expression VisitMemberInit(MemberInitExpression memInit)
        {
            var newExpression = Visit(memInit.NewExpression).Cast<NewExpression>();
            var bindings = new List<MemberBinding>();

            foreach (var item in memInit.Bindings.OfType<MemberAssignment>())
            {
                var bindExpression = Visit(item.Expression);

                var navigation = _queryContext.PropertyNavigationContext.GetNavigation(item.Member);
                if (navigation != null)
                {
                    var condtion = Visit(navigation.Relation).UnWrap().As<LambdaExpression>();
                    var elementType = item.Member.GetMemberInfoType().GetCollectionElementType();

                    var lambda = Expression.Lambda(condtion.Body, condtion.Parameters.LastOrDefault());
                    var filter = Expression.Call(null, ReflectionUtil.EnumerableWhereMethod.MakeGenericMethod(elementType), bindExpression, lambda);
                    var toList = Expression.Call(null, ReflectionUtil.EnumerableToListMethod.MakeGenericMethod(elementType), filter);

                    bindExpression = toList;
                }

                bindings.Add(Expression.Bind(item.Member, bindExpression));
            }
            return memInit.Update(newExpression, bindings);
        }

        public Delegate CreateQueryModelActivateDelegate(Expression modelExpression, Expression source, QueryContext context = null)
        {
            _queryContext = context;
            _source = source;

            modelExpression = Visit(modelExpression).UnWrap();
            if (modelExpression.NodeType == ExpressionType.Lambda)
            {
                modelExpression = modelExpression.As<LambdaExpression>().Body;
            }

            return Expression.Lambda(modelExpression, EntityMemberActivator.ActivateMethodParameter).Compile();
        }
    }
}
