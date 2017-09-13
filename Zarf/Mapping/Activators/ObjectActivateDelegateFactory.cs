using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query;

namespace Zarf.Mapping
{
    public class ObjectActivateDelegateFactory : ExpressionVisitor
    {
        private IQueryContext _context;

        private Expression _rootSource;

        public static ParameterExpression @This { get; set; }

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
            var map = _context.ProjectionMappingProvider.GetMapping(node);
            if (map != null && map.Source == _rootSource)
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

        protected override Expression VisitParameter(ParameterExpression param)
        {
            if (param == P)
            {
                return This;
            }

            return param;
        }

        protected ParameterExpression P { get; set; }

        protected override Expression VisitMemberInit(MemberInitExpression memInit)
        {
            var newExpression = Visit(memInit.NewExpression).Cast<NewExpression>();
            var bindings = new List<MemberBinding>();

            foreach (var item in memInit.Bindings.OfType<MemberAssignment>())
            {
                var bindExpression = Visit(item.Expression);

                var navigation = _context.PropertyNavigationContext.GetNavigation(item.Member);
                if (navigation != null && This != null)
                {
                    //var condtion = Visit(navigation.Relation).UnWrap().As<LambdaExpression>();
                    var condtion = navigation.Relation.UnWrap().As<LambdaExpression>();
                    P = condtion.Parameters.First();
                    var elementType = item.Member.GetMemberInfoType().GetCollectionElementType();

                    condtion = Expression.Lambda(condtion.Body, This, condtion.Parameters.Last());

                    var lambda = Visit(condtion);
                    var filter = Expression.Call(null, ReflectionUtil.EnumerableWhereMethod.MakeGenericMethod(This.Type, elementType), bindExpression, This, lambda);
                    var toList = Expression.Call(null, ReflectionUtil.EnumerableToListMethod.MakeGenericMethod(elementType), filter);

                    bindExpression = toList;
                }

                bindings.Add(Expression.Bind(item.Member, bindExpression));
            }

            return memInit.Update(newExpression, bindings);
        }

        public Delegate CreateQueryModelActivateDelegate(Expression modelExpression, Expression source, IQueryContext context = null)
        {
            lock (this)
            {
                _context = context;
                _rootSource = source;
                modelExpression = Visit(modelExpression).UnWrap();
            }

            if (modelExpression.NodeType == ExpressionType.Lambda)
            {
                modelExpression = modelExpression.As<LambdaExpression>().Body;
            }
            //var container=MemberInitExpression
            //container.AAA=new AAA();
            //return container;

            //var result = Expression.Lambda(modelExpression, EntityMemberActivator.ActivateMethodParameter).Compile();
            var target = Expression.Label(modelExpression.Type);
            This = Expression.Variable(modelExpression.Type);
            var setLocal = Expression.Assign(This, modelExpression);

            var exp = modelExpression.As<MemberInitExpression>();
            var binding = exp.Bindings.Last().As<MemberAssignment>();

            if (binding.Member.GetMemberInfoType().IsCollection())
            {

                //begin
                var navigation = _context.PropertyNavigationContext.GetNavigation(binding.Member);

                var condtion = navigation.Relation.UnWrap().As<LambdaExpression>();
                P = condtion.Parameters.First();
                var elementType = binding.Member.GetMemberInfoType().GetCollectionElementType();

                condtion = Expression.Lambda(condtion.Body, This, condtion.Parameters.Last());

                var lambda = Visit(condtion);
                var filter = Expression.Call(null, ReflectionUtil.EnumerableWhereMethod.MakeGenericMethod(This.Type, elementType), binding.Expression, This, lambda);
                var toList = Expression.Call(null, ReflectionUtil.EnumerableToListMethod.MakeGenericMethod(elementType), filter);
                //end

                var call = Expression.Call(This, binding.Member.As<PropertyInfo>().SetMethod,filter);

                var ret = Expression.Return(target, This, modelExpression.Type);
                var label = Expression.Label(target, This);
                var block = Expression.Block(new[] { This }, setLocal, call, ret, label);

                return Expression.Lambda(block, EntityMemberActivator.ActivateMethodParameter).Compile();
            }
            else
            {
                var ret = Expression.Return(target, This, modelExpression.Type);
                var label = Expression.Label(target, This);
                var block = Expression.Block(new[] { This }, setLocal, ret, label);

                return Expression.Lambda(block, EntityMemberActivator.ActivateMethodParameter).Compile();
            }
        }
    }
}
