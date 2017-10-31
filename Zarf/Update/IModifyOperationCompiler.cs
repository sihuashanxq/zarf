using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Zarf.Update
{
    public interface IModifyOperationCompiler
    {
        IEnumerable<DbModifyCommand> Compile(DbModifyOperation modifyOperation);
    }

    public class ModifyOperationCompiler : IModifyOperationCompiler
    {
        public IEnumerable<DbModifyCommand> Compile(DbModifyOperation modifyOperation)
        {
            var modifyCommands = new List<DbModifyCommand>();
            foreach (var entity in modifyOperation.Entities)
            {
                var modifyCommand = Compile(entity, modifyOperation.Predicate);
                if (modifyCommand != null)
                {
                    modifyCommands.Add(modifyCommand);
                }
            }

            return modifyCommands;
        }

        public DbModifyCommand Compile(EntityEntry entity, LambdaExpression predicate)
        {
            if (predicate == null)
            {

            }
            return null;
        }

        public LambdaExpression BuildModifyPredicate(EntityEntry entity, LambdaExpression predicate)
        {
            //TODO BUILD Predicate
            //alone visit
            //alone sql builder
            if (predicate == null)
            {
                return predicate;
            }

            if (entity.PrimaryMember != null)
            {
                var primaryEqual = Expression.Equal(
                    Expression.MakeMemberAccess(Expression.Constant(entity.Entity), entity.PrimaryMember.Member),
                    Expression.Constant(entity.PrimaryMember.GetValue(entity.Entity))
                );

                return Expression.Lambda(primaryEqual);
            }

            return null;
        }
    }
}
