using Simp.AST;
using Simp.Common;
using Simp.StaticAnalysis;

namespace Simp.CodeGeneration.LLVM
{
    public partial class Builder
    {
        void BuildDeclaration(Declaration declaration)
        {
            switch (declaration)
            {
                case FunctionDeclaration f:
                    BuildFunction(f);
                    break;
                default:
                    throw new TokenError("Unknown declaration type.", declaration.SourceStart);
            }
        }

        void BuildFunction(FunctionDeclaration f)
        {
            TempReg = 0;
            CurrentFunction = new Function(
                f.FunctionName,
                Int64.Instance,
                f.ArgumentList.Select((_) => Int64.Instance).ToList<DataType>());

            Resolver = new MemResolver();
            Resolver.EnterScope();
            foreach (var argument in f.ArgumentList)
            {
                if (Resolver.DeclareFunctionArgument(argument) == null)
                {
                    throw new TokenError($"Argument '{argument}' already defined.", f.SourceStart);
                }
            }

            CurrentLabel = new Label("entry");

            BuildBlock(f.Body);
            Resolver.ExitScope();

            if (CurrentLabel.Ops.Count == 0 || CurrentLabel.Ops[^1] is not Ret)
            {
                CurrentLabel.Add(new Ret(Int64.Instance, "0"));
            }

            CurrentFunction.Labels.Add(CurrentLabel);
            Declarations.Add(CurrentFunction);
        }
    }
}