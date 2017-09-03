﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Zarf.Query
{
    public class QueryContextFacotry : IQueryContextFactory
    {
        public static readonly IQueryContextFactory Factory = new QueryContextFacotry();

        private QueryContextFacotry()
        {

        }

        public IQueryContext CreateContext()
        {
            //di ProviderFactory
            return new QueryContext(
                    new EntityMemberSourceMappingProvider(),
                    new PropertyNavigationContext(),
                    new QuerySourceProvider(), 
                    new ExpressionVisitorProjectionFinder(),
                    new AliasGenerator()
                );
        }
    }
}
