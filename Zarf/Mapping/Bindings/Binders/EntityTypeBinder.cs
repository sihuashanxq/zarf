using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Mapping.Bindings
{
    public class EntityTypeBinder : ExpressionVisitor, IEntityBinder
    {
        public Expression Bind(IBindingContext bindingContext)
        {
            if (bindingContext.BindExpression != null)
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
            var block = Visit(node.NewExpression) as BlockExpression;
            var members = new List<MemberInfo>();
            var expressions = new List<Expression>();

            foreach (var binding in node.Bindings.OfType<MemberAssignment>())
            {
                members.Add(binding.Member);
                expressions.Add(binding.Expression);
            }

            return BindMembers(block, members, expressions);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var constructor = node.Constructor;
            if (node.Constructor.GetParameters().Length != 0)
            {
                constructor = node.Type.GetConstructor(Type.EmptyTypes);
            }

            var block = CreateEntityNewExpressionBlock(constructor, node.Type);
            if (node.Arguments.Count == 0)
            {
                return block;
            }

            return BindMembers(block, node.Members.ToList(), node.Arguments.ToList());
        }

        protected BlockExpression BindMembers(BlockExpression entityCreationBlock, List<MemberInfo> bindMembers, List<Expression> bindExpressions)
        {
            var bindings = new List<Expression>();
            var entity = entityCreationBlock.Variables.FirstOrDefault();

            for (var i = 0; i < bindMembers.Count; i++)
            {
                var context = new BindingContext(entity.Type, entity, bindMembers[i], bindExpressions?[i]);
                var binder = EntityBinderProvider.Default.GetBinder(context);
                var binding = binder?.Bind(context);
                if (binding == null)
                {
                    continue;
                }

                bindings.Add(binding);
            }

            var expressions = entityCreationBlock.Expressions.ToList();
            var insertIndex = expressions.FindLastIndex(item => item is GotoExpression);
            if (insertIndex > 0)
            {
                expressions.InsertRange(insertIndex, bindings);
            }

            return entityCreationBlock.Update(entityCreationBlock.Variables, expressions);
        }

        protected Expression BindMember(Expression entity, MemberInfo bindMember, Expression bindExpression = null)
        {
           
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
        private static BlockExpression CreateEntityNewExpressionBlock(ConstructorInfo constructor, Type type)
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
