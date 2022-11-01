namespace Simp.CodeGeneration.LLVM
{
    public abstract class DataType
    {
        public override string ToString()
        {
            return Generate();
        }
        public abstract string Generate();
    }

    public class Int64 : DataType
    {
        public static readonly Int64 Instance = new();

        public override string Generate()
        {
            return "i64";
        }
    }

    public class I1 : DataType
    {
        public static readonly I1 Instance = new();

        public override string Generate()
        {
            return "i1";
        }
    }

    public class Pointer : DataType
    {
        public static readonly Pointer Int64Ptr = new(Int64.Instance);
        public static readonly Pointer BoolPtr = new(I1.Instance);

        public Pointer(DataType type)
        {
            BaseType = type;
        }

        public DataType BaseType { get; set; }

        public override string Generate()
        {
            return $"{BaseType.Generate()}*";
        }
    }
}