namespace Simp.CodeGeneration.LLVM
{
    public abstract class DataType
    {

    }

    public class Int64
    {
        public static readonly Int64 Instance = new();
    }

    public class Pointer<T>
    {
        public static readonly Pointer<Int64> Int64Ptr = new(Int64.Instance);

        public Pointer(T type)
        {
            BaseType = type;
        }

        public T BaseType { get; set; }
    }
}