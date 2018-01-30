using System;
using System.Data.SQLite;

namespace Zarf.Sqlite.Functions
{
    [SQLiteFunction(Name = "DateTimeAddTicks", FuncType = FunctionType.Scalar, Arguments = 2)]
    public class DateTimeAddTicks : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Convert.ToDateTime(args[0]).AddTicks(Convert.ToInt64(args[1]));
        }
    }
}
