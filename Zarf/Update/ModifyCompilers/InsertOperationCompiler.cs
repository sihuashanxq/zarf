using System.Collections.Generic;
using Zarf.Entities;
using Zarf.Update.Commands;

namespace Zarf.Update.Compilers
{
    public class InsertOperationCompiler : ModifyOperationCompiler
    {
        public override DbModifyCommand Compile(EntityEntry entry, MemberDescriptor identity)
        {
            var columns = new List<string>();
            var dbParams = new List<DbParameter>();

            foreach (var item in entry.Members)
            {
                if (item.IsIncrement)
                {
                    continue;
                }

                columns.Add(GetColumnName(item));
                dbParams.Add(GetDbParameter(entry.Entity, item));
            }

            return new DbInsertCommand(entry, columns, dbParams);
        }
    }
}
