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

    public class MathOp : RegOp
    {
        public enum Operator
        {
            add,
            sub,
            mul,
            sdiv
        }

        public Operator Op { get; private set; }

        public MathOp(
            string reg1,
            string reg2,
            string result,
            DataType type,
            Operator op
        ) : base(reg1, reg2, result, type)
        {
            Op = op;
        }

        public override void Generate(StreamWriter writer)
        {
            writer.WriteLine($"{ResultReg} = {Op} {DataType} {Reg1}, {Reg2}");
        }
    }

    public class Comp : RegOp
    {
        public enum Condition
        {
            eq,
            ne,
            ugt,
            uge,
            ult,
            ule,
            sgt,
            sge,
            slt,
            sle
        }

        public Condition Cond { get; private set; }

        public Comp(
            string reg1,
            string reg2,
            string result,
            DataType type,
            Condition cond
        ) : base(reg1, reg2, result, type)
        {
            Cond = cond;
        }

        public override void Generate(StreamWriter writer)
        {
            writer.WriteLine($"{ResultReg} = icmp {Cond} {DataType} {Reg1}, {Reg2}");
        }
    }

    public class Extension : LLVMOp
    {
        public enum ExtensionType
        {
            trunc,
            zext,
            sext,
            fptrunc,
            fpext,
            fptoui,
            uitofp,
            sitofp,
            ptrtoint,
            bitcast,
            addrspacecast
        }

        public string Reg { get; private set; }
        public string Result { get; private set; }
        public DataType SourceType { get; private set; }
        public DataType DestType { get; private set; }
        public ExtensionType Ext { get; private set; }

        public Extension(
            string reg,
            string result,
            DataType sourceType,
            DataType destType,
            ExtensionType ext
        )
        {
            Reg = reg;
            Result = result;
            SourceType = sourceType;
            DestType = destType;
            Ext = ext;
        }

        public override void Generate(StreamWriter writer)
        {
            writer.WriteLine($"{Result} = {Ext} {SourceType} {Reg} to {DestType}");
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

    public class ConditionalBranch : LLVMOp
    {
        public string TrueLabel { get; private set; }
        public string FalseLabel { get; private set; }
        public string Reg { get; private set; }
        public DataType DataType { get; private set; }

        public ConditionalBranch(
            string reg,
            DataType dataType,
            string trueLabel,
            string falseLabel)
        {
            Reg = reg;
            DataType = dataType;
            TrueLabel = trueLabel;
            FalseLabel = falseLabel;
        }

        public override void Generate(StreamWriter writer)
        {
            writer.WriteLine($"br {DataType} {Reg}, label %{TrueLabel}, label %{FalseLabel}");
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