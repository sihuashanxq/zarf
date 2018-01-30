using System;
using System.Data.SQLite;

namespace Zarf.Sqlite.Functions
{
    [SQLiteFunction(Name = "Exp", FuncType = FunctionType.Scalar, Arguments = 1)]
    public class ExpFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Exp(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Asin", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class AsinFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Asin(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Acos", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class AcosFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Acos(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Atan", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class AtanFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Atan(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Cos", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class CosFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Cos(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Ceiling", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class CeilingFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Ceiling(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Floor", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class FloorFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Floor(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Log10", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class Log10Function : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Log10(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Sign", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class SignFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Sign(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Sqrt", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class SqrtFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Sqrt(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Tan", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class TanFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Tan(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Atan2", Arguments = 2, FuncType = FunctionType.Scalar)]
    public class Atan2Function : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Atan2(Convert.ToDouble(args[0]), Convert.ToDouble(args[2]));
        }
    }

    [SQLiteFunction(Name = "Log", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class LogFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Log(Convert.ToDouble(args[0]));
        }
    }

    [SQLiteFunction(Name = "Log2", Arguments = 2, FuncType = FunctionType.Scalar)]
    public class Log2Function : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Log(Convert.ToDouble(args[0]), Convert.ToDouble(args[2]));
        }
    }

    [SQLiteFunction(Name = "Pow", Arguments = 2, FuncType = FunctionType.Scalar)]
    public class PowFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[2]));
        }
    }
}
