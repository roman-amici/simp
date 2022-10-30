namespace Simp.CodeGeneration.LLVM
{
    public abstract class LLVMOp
    {
    }

    public class Label : LLVMOp
    {
        public string Tag { get; private set; }
        public List<LLVMOp> Ops { get; private set; } = new List<LLVMOp>();
        public Label? Next { get; set; }

        public Label(string tag)
        {
            Tag = tag;
        }

        public void Add(LLVMOp op)
        {
            Ops.Add(op);
        }
    }

    public class Function : LLVMOp
    {
        public List<Label> Labels { get; private set; } = new List<Label>();
        public DataType ReturnType { get; private set; }
        public IList<DataType> Arguments { get; private set; }

        public Function(DataType returnType, IList<DataType> arguments)
        {
            ReturnType = returnType;
            Arguments = arguments;
        }
    }

    public abstract class RegOp
    {
        public string Reg1 { get; private set; }
        public string Reg2 { get; private set; }
        public string ResultReg { get; private set; }
        public DataType DataType { get; private set; }

        public RegOp(
            string reg1,
            string reg2,
            string result,
            DataType type)
        {
            Reg1 = reg1;
            Reg2 = reg2;
            ResultReg = result;
            DataType = type;
        }
    }

    public class Branch
    {
        public string Tag { get; private set; }
        public Branch(string tag)
        {
            Tag = tag;
        }
    }

    public class Ret
    {
        public DataType ReturnType { get; private set; }
        public string RegName { get; private set; }
        public Ret(DataType returnType, string reg)
        {
            ReturnType = returnType;
            RegName = reg;
        }
    }
}