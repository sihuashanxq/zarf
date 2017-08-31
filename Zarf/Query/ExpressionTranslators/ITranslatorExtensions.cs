using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Mapping;
using Zarf.Query.Expressions;

namespace Zarf.Query.ExpressionTranslators
{
    public static class ITranslatorExtensions
    {
        public static IEnumerable<Expression> GenerateTableColumns(this ITranslaor _, FromTableExpression table)
        {
            var entityType = EntityTypeDescriptorFactory.Factory.Create(table.Type);
            foreach (var member in entityType.GetWriteableMembers())
            {
                yield return new ColumnExpression(table, member, member.Name);
            }
        }
    }
}
