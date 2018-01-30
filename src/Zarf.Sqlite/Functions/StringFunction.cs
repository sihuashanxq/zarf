using System.Data.SQLite;

namespace Zarf.Sqlite.Functions
{
    [SQLiteFunction(Name = "CharIndex", FuncType = FunctionType.Scalar, Arguments = 2)]
    public class StringIndexOfFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return args[1].ToString().IndexOf(args[0].ToString()) + 1;
        }
    }
}
