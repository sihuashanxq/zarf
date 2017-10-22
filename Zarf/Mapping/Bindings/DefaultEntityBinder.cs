using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Mapping.Bindings.Binders;
using Zarf.Query.Expressions;

namespace Zarf.Mapping.Bindings
{
    /// <summary>
    /// 默认实体绑定实现
    /// </summary>
    public class DefaultEntityBinder : ExpressionVisitor, IBinder
    {
        public static readonly ParameterExpression DataReader = Expression.Parameter(typeof(IDataReader));

        public IEntityProjectionMappingProvider MappingProvider { get; }

        public Expression Root { get; }

        public DefaultEntityBinder(IEntityProjectionMappingProvider provider, Expression root)
        {
            Root = root;
            MappingProvider = provider;
        }

        public Expression Bind(IBindingContext context)
        {
            return Visit(context.BindExpression);
        }

        protected override Expression VisitMemberInit(MemberInitExpression memInit)
        {
            var eNewBlock = Visit(memInit.NewExpression) as BlockExpression;
            return BindMembers(eNewBlock,
                memInit.Bindings.OfType<MemberAssignment>().Select(item => item.Member).ToList(),
                memInit.Bindings.OfType<MemberAssignment>().Select(item => item.Expression).ToList());
        }

        protected override Expression VisitExtension(Expression exp)
        {
            ColumnExpression column = null;
            if (exp.Is<ColumnExpression>())
            {
                column = exp.As<ColumnExpression>();
            }

            if (exp.Is<AggregateExpression>())
            {
                column = exp.As<AggregateExpression>().KeySelector.As<ColumnExpression>();
            }

            if (column != null)
            {
                var ordinal = MappingProvider.GetOrdinal(Root, column);
                var valueSetter = MemberValueGetterProvider.Default.GetValueGetter(column.Type);
                if (ordinal == -1 || valueSetter == null)
                {
                    throw new NotImplementedException($"列{column.Column.Name} 未包含在查询中!");
                }

                return Expression.Call(null, valueSetter, DataReader, Expression.Constant(ordinal));
            }

            if (exp.Is<FromTableExpression>())
            {
                return BindQueryExpression(exp.As<QueryExpression>());
            }
            else
            {
                throw new NotImplementedException($"不支持{exp.GetType().Name} 到 {exp.Type.Name}的转换!!!");
            }
        }

        protected override Expression VisitNew(NewExpression newExp)
        {
            var constructorInfo = newExp.Constructor;
            if (newExp.Constructor.GetParameters().Length != 0)
            {
                constructorInfo = newExp.Type.GetConstructor(Type.EmptyTypes);
            }

            var eNewBlock = CreateEntityNewExpressionBlock(constructorInfo, newExp.Type);
            if (newExp.Arguments.Count == 0)
            {
                return eNewBlock;
            }

            return BindMembers(eNewBlock, newExp.Members.ToList(), newExp.Arguments.ToList());
        }

        protected BlockExpression BindMembers(BlockExpression eNewBlock, List<MemberInfo> mems, List<Expression> expes)
        {
            var memBindings = new List<Expression>();
            var entity = eNewBlock.Variables.FirstOrDefault();

            for (var i = 0; i < mems.Count; i++)
            {
                var argment = Visit(expes[i]);
                var memAccess = Expression.MakeMemberAccess(eNewBlock.Variables.FirstOrDefault(), mems[i]);
                memBindings.Add(Expression.Assign(memAccess, argment));
            }

            var nodes = eNewBlock.Expressions.ToList();
            var retIndex = nodes.FindLastIndex(item => item is GotoExpression);
            if (retIndex == -1)
            {
                throw new Exception();
            }

            nodes.InsertRange(retIndex, memBindings);
            return eNewBlock.Update(eNewBlock.Variables, nodes);
        }

        protected Expression BindQueryExpression(QueryExpression query)
        {
            if (query == null)
            {
                return null;
            }

            var typeDescriptor = EntityTypeDescriptorFactory.Factory.Create(query.Type);
            var memExpressions = new List<Expression>();
            var members = new List<MemberInfo>();
            var eNewBlock = CreateEntityNewExpressionBlock(typeDescriptor.Constructor, typeDescriptor.Type);

            foreach (var item in typeDescriptor.GetWriteableMembers())
            {
                var bindExpression = FindMemberRelatedExpression(query, item);
                if (bindExpression != null)
                {
                    memExpressions.Add(bindExpression);
                    members.Add(item);
                }
            }

            return BindMembers(eNewBlock, members, memExpressions);
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

        public static Expression FindMemberRelatedExpression(QueryExpression query, MemberInfo member)
        {
            if (query.ProjectionCollection?.Count == 0 &&
                query.SubQuery != null)
            {
                return FindMemberRelatedExpression(query.SubQuery, member);
            }

            return query.ProjectionCollection.FirstOrDefault(item => item.Member == member)?.Expression;
        }
    }
}
