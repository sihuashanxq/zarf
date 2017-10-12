using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Mapping.Bindings
{
    /// <summary>
    /// Member 绑定不科学
    /// 不可取
    /// 仍然采用Expression绑定更好些
    /// </summary>
    public class EntityTypeBinder : ExpressionVisitor, IEntityBinder
    {
        //TODO
        protected IBindingContext BindingContext { get; set; }

        public Expression Bind(IBindingContext bindingContext)
        {
            BindingContext = bindingContext;

            if (BindingContext.BindExpression != null && !BindingContext.BindExpression.Is<QueryExpression>())
            {
                return Visit(bindingContext.BindExpression) as BlockExpression;
            }

            var descriptor = EntityTypeDescriptorFactory.Factory.Create(bindingContext.Type);
            if (descriptor.Constructor == null)
            {
                throw new NullReferenceException("the entity must have a consctrucotr with none arguments");
            }

            var block = CreateEntityNewExpressionBlock(descriptor.Constructor, bindingContext.Type);
            return BindMembers(block, descriptor.GetWriteableMembers().ToList(), null);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var eNewBlock = Visit(node.NewExpression) as BlockExpression;
            var members = new List<MemberInfo>();
            var expressions = new List<Expression>();

            foreach (var binding in node.Bindings.OfType<MemberAssignment>())
            {
                //translate binding.Expression
                //Set Member=BindingExpression
                members.Add(binding.Member);
                expressions.Add(binding.Expression);
            }

            return BindMembers(eNewBlock, members, expressions);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var constructor = node.Constructor;
            if (node.Constructor.GetParameters().Length != 0)
            {
                constructor = node.Type.GetConstructor(Type.EmptyTypes);
            }

            var eNewBlock = CreateEntityNewExpressionBlock(constructor, node.Type);
            if (node.Arguments.Count == 0)
            {
                return eNewBlock;
            }

            return BindMembers(eNewBlock, node.Members.ToList(), node.Arguments.ToList());
        }

        protected BlockExpression BindMembers(BlockExpression eNewBlock, List<MemberInfo> bindMembers, List<Expression> bindExpressions)
        {
            var bindings = new List<Expression>();
            var entity = eNewBlock.Variables.FirstOrDefault();

            for (var i = 0; i < bindMembers.Count; i++)
            {
                var bindingContext = BindingContext.CreateMemberBindingContext(entity.Type, entity, bindMembers[i], bindExpressions?[i]);
                var binder = EntityBinderProviders.GetBinder(bindingContext);
                var binding = binder?.Bind(bindingContext);
                if (binding == null)
                {
                    continue;
                }
                bindings.Add(binding);
            }

            var blockExpressions = eNewBlock.Expressions.ToList();
            var retIndex = blockExpressions.FindLastIndex(item => item is GotoExpression);
            if (retIndex == -1)
            {
                throw new Exception();
            }

            blockExpressions.InsertRange(retIndex, bindings);
            return eNewBlock.Update(eNewBlock.Variables, blockExpressions);
        }

        /// <summary>
        /// {
        ///     var entity=new Entity();
        ///     return entity;
        /// }
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static BlockExpression CreateEntityNewExpressionBlock(ConstructorInfo constructor, Type type)
        {
            if (constructor == null)
            {
                throw new NotImplementedException($"Type:{type.FullName} need a conscrutor which is none of parameters!");
            }

            var begin = Expression.Label(type);

            var var = Expression.Variable(type);
            var varValue = Expression.Assign(var, Expression.New(constructor));
            var retVar = Expression.Return(begin, var);

            var end = Expression.Label(begin, var);
            return Expression.Block(new[] { var }, varValue, retVar, end);
        }
    }
}
