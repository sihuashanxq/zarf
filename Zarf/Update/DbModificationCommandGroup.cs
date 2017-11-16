using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zarf.Entities;

namespace Zarf.Update
{
    public class DbModificationCommandGroup
    {
        public List<DbModificationCommand> Commands { get; }

        public int DbParameterCount
        {
            get
            {
                return Commands?.Sum(item => item.DbParameterCount) ?? 0;
            }
        }

        public DbModificationCommandGroup()
        {
            Commands = new List<DbModificationCommand>();
        }

        public IEnumerable<DbParameter> Parameters
        {
            get
            {
                var parameters = new List<DbParameter>();
                foreach (var item in Commands)
                {
                    if (item.DbParams != null)
                    {
                        parameters.AddRange(item.DbParams);
                    }

                    if (item.PrimaryKeyValues != null)
                    {
                        parameters.AddRange(item.PrimaryKeyValues);
                    }
                }

                return parameters;
            }
        }
    }
}
