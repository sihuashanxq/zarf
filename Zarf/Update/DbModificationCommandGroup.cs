using System.Collections.Generic;
using System.Linq;

using Zarf.Metadata.Entities;

namespace Zarf.Update
{
    public class DbModificationCommandGroup
    {
        public List<DbModificationCommand> Commands { get; }

        public int ParameterCount
        {
            get
            {
                return Commands?.Sum(item => item.ParameterCount) ?? 0;
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
                    if (item.Parameters != null)
                    {
                        parameters.AddRange(item.Parameters);
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
