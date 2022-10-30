namespace Simp.CodeGeneration.LLVM
{
    public abstract class LLVMOp
    {
        public abstract void Generate(StreamWriter writer);
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

        public override void Generate(StreamWriter writer)
        {
            writer.WriteLine($"{Tag}:");
            foreach (var op in Ops)
            {
                op.Generate(writer);
            }
        }
    }

    public abstract class LLVMDeclaration : LLVMOp
    {

    }

    public class Function : LLVMDeclaration
    {
        public List<Label> Labels { get; private set; } = new List<Label>();
        public DataType ReturnType { get; private set; }
        public IList<DataType> Arguments { get; private set; }
        public string Name { get; private set; }

        public Function(
            string name,
            DataType returnType,
            IList<DataType> arguments)
        {
            Name = name;
            ReturnType = returnType;
            Arguments = arguments;
        }

        public override void Generate(StreamWriter writer)
        {
            var argumentList = string.Join(
                ',', Arguments.Select(d => d.Generate()));
            writer.WriteLine($"define {ReturnType} @{Name}({argumentList}) {{");

            foreach (var label in Labels)
            {
                label.Generate(writer);
            }

            writer.WriteLine("}");
        }
    }

    public abstract class RegOp : LLVMOp
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

    public class Branch : LLVMOp
    {
        public string Tag { get; private set; }
        public Branch(string tag)
        {
            Tag = tag;
        }

        public override void Generate(StreamWriter writer)
        {
            writer.WriteLine($"br label %{Tag}");
        }
    }

    public class Ret : LLVMOp
    {
        public DataType ReturnType { get; private set; }
        public string RegName { get; private set; }
        public Ret(DataType returnType, string reg)
        {
            ReturnType = returnType;
            RegName = reg;
        }

        public override void Generate(StreamWriter writer)
        {
            writer.WriteLine($"ret {ReturnType.Generate()} {RegName}");
        }
    }
}