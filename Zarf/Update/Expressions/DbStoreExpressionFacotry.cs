using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Update.Expressions
{
    public class DbStoreExpressionFacotry
    {
        public static readonly DbStoreExpressionFacotry Default = new DbStoreExpressionFacotry();

        private DbStoreExpressionFacotry()
        {
        }

        public DbStoreExpression Create(DbModificationCommandGroup group)
        {
            var persists = new List<Expression>();
            foreach (var command in group.Commands)
            {
                switch (command.Entry.State)
                {
                    case EntityState.Insert:
                        persists.Add(new InsertExpression(
                            command.Entry.Type.ToTable(),
                            command.DbParams,
                            command.Columns,
                            command.Entry.AutoIncrementProperty != null
                        ));
                        break;
                    case EntityState.Update:
                        persists.Add(new UpdateExpression(
                            command.Entry.Type.ToTable(),
                            command.DbParams,
                            command.Columns,
                            command.PrimaryKey,
                            command.PrimaryKeyValues.FirstOrDefault()
                        ));
                        break;
                    default:
                        persists.Add(new DeleteExpression(
                            command.Entry.Type.ToTable(),
                            command.PrimaryKey,
                            command.PrimaryKeyValues
                        ));
                        break;
                }
            }

            return new DbStoreExpression(persists);
        }
    }
}
