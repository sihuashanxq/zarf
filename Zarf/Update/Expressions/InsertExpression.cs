﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Zarf.Entities;

namespace Zarf.Query.Expressions
{
    public class InsertExpression : Expression
    {
        public override Type Type => typeof(void);

        public override ExpressionType NodeType => ExpressionType.Extension;

        public IEnumerable<DbParameter> DbParams { get; }

        public IEnumerable<string> Columns { get; }

        public Table Table { get; }

        public MemberInfo IncrementMember { get; }

        public InsertExpression(Table table, IEnumerable<DbParameter> dbParams, IEnumerable<string> columns, MemberInfo incrementMember = null)
        {
            Table = table;
            DbParams = dbParams;
            Columns = columns;
            IncrementMember = incrementMember;
        }
    }
}