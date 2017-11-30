using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators
{
    public static class ITranslatorExtensions
    {
        public static IEnumerable<ColumnExpression> GenerateTableColumns(this ITranslaor _, FromTableExpression table)
        {
            var typeOfEntity = TypeDescriptorCacheFactory.Factory.Create(table.Type);
            foreach (var memberDescriptor in typeOfEntity.MemberDescriptors)
            {
                yield return new ColumnExpression(
                    table,
                    memberDescriptor.Member,
                    memberDescriptor.Name);
            }
        }
    }
}
