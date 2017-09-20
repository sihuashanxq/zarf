using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Zarf.Mapping.Bindings
{
    public class EntityTypeBinder : IEntityBinder
    {
        public Expression Bind(IBindingContext bindingContext)
        {
            var entityDescriptor = EntityTypeDescriptorFactory.Factory.Create(bindingContext.Type);
            if (entityDescriptor.Constructor == null)
            {
                throw new NullReferenceException("the entity must have a consctrucotr with none arguments");
            }

            var entityNewBlock = CreateEntityNewExpressionBlock(entityDescriptor.Constructor, bindingContext.Type);
            var entityObject = entityNewBlock.Variables.FirstOrDefault();

            var bindings = BindMember(bindingContext, entityDescriptor, entityObject);

            var statements = entityNewBlock.Expressions.ToList();
            var insertIndex = statements.FindLastIndex(item => item is GotoExpression);
            if (insertIndex > 0)
            {
                statements.InsertRange(insertIndex - 1, bindings);
            }

            return entityNewBlock.Update(entityNewBlock.Variables, statements);
        }

        private List<Expression> BindMember(IBindingContext bindingContext, EntityTypeDescriptor typeDescriptor, Expression entityObject)
        {
            var bindings = new List<Expression>();

            foreach (var member in typeDescriptor.GetWriteableMembers())
            {
                var memberBindingContext = new BindingContext(bindingContext.Type, entityObject, member);
                var binder = EntityBinderProvider.Default.GetBinder(memberBindingContext);
                if (binder == null)
                {
                    continue;
                }

                var binding = binder.Bind(memberBindingContext);
                if (binding != null)
                {
                    bindings.Add(binding);
                }
            }

            return bindings;
        }

        /// <summary>
        /// {
        ///     var entity=new Entity();
        ///     return entity;
        /// }
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private BlockExpression CreateEntityNewExpressionBlock(ConstructorInfo constructor, Type entityType)
        {

            var beginBlock = Expression.Label(entityType);

            var entityVar = Expression.Variable(entityType);
            var assignEntityVarValue = Expression.Assign(entityVar, Expression.New(constructor));
            var retEntityVar = Expression.Return(beginBlock, entityVar);

            var endBlock = Expression.Label(beginBlock, entityVar);

            return Expression.Block(
                new[] { entityVar },
                assignEntityVarValue,
                retEntityVar,
                endBlock);
        }
    }
}
