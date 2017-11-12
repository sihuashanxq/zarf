using System.Collections.Generic;
using System.Reflection;
using Zarf.Entities;
using Zarf.Update.Commands;

namespace Zarf.Update.Compilers
{
    public class UpdateOperationCompiler : ModifyOperationCompiler
    {
        private Dictionary<object, Dictionary<MemberInfo, object>> _trackEntityValues;

        public UpdateOperationCompiler(Dictionary<object, Dictionary<MemberInfo, object>> trackEntityValues)
        {
            _trackEntityValues = trackEntityValues;
        }

        public override DbModifyCommand Compile(EntityEntry entry, MemberDescriptor identity)
        {
            var columns = new List<string>();
            var dbParams = new List<DbParameter>();
            var entityMemValues = _trackEntityValues.GetValueOrDefault(entry.Entity);

            foreach (var item in entry.Members)
            {
                if (item.IsIncrement || item.IsPrimary || item.Member == identity.Member)
                {
                    continue;
                }

                var dbParameter = GetDbParameter(entry.Entity, item);
                if (entityMemValues != null)
                {
                    //track
                    var memValue = entityMemValues[item.Member];
                    if (memValue == null && dbParameter.Value == null)
                    {
                        continue;
                    }

                    if (memValue != null && memValue.Equals(dbParameter.Value))
                    {
                        continue;
                    }

                    if (dbParameter.Value != null && dbParameter.Equals(dbParameter.Value))
                    {
                        continue;
                    }

                    //update track
                    entityMemValues[item.Member] = dbParameter.Value;
                }

                columns.Add(GetColumnName(item));
                dbParams.Add(dbParameter);
            }

            return new DbUpdateCommand(entry, columns, dbParams, GetColumnName(identity), GetDbParameter(entry.Entity, identity));
        }
    }
}
