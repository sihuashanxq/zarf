using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Zarf.Core;

namespace Zarf.SqlServer.Core
{
    public class SqlServerDataBaseFacade : IDataBaseFacade
    {
        private IDbCommandFacade _dbCommandFacade;

        public SqlServerDataBaseFacade(IDbCommandFacade dbCommandFacade)
        {
            _dbCommandFacade = dbCommandFacade;
        }

        public IDbCommandFacade GetCommand()
        {
            return _dbCommandFacade;
        }
    }
}
