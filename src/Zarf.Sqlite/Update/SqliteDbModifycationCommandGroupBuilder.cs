using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zarf.Update;

namespace Zarf.Sqlite.Update
{
    public class SqliteDbModifycationCommandGroupBuilder : DbModificationCommandGroupBuilder
    {
        protected override int MaxParameterCount => 999;

        public SqliteDbModifycationCommandGroupBuilder(IEntityTracker tracker)
            : base(tracker)
        {

        }

        protected override DbModificationCommandGroup FindCommadGroup(List<DbModificationCommandGroup> groups, DbModificationCommand modifyCommand)
        {
            //类型与操作相同才能合并
            var group = groups.LastOrDefault();
            if (group != null &&
                group.ParameterCount + modifyCommand.ParameterCount < MaxParameterCount)
            {
                if ((modifyCommand.State != EntityState.Insert || modifyCommand.Entry.AutoIncrementProperty == null) &&
                    group.Commands.Any(item => item.State != EntityState.Insert || item.Entry.AutoIncrementProperty == null) &&
                    group.Commands.All(item => item.Entry.Type == modifyCommand.Entry.Type && item.Entry.State == modifyCommand.State))
                {
                    return group;
                }
            }

            return null;
        }
    }
}
