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

        void EnterFunction(Function fn)
        {
            GlobalVariables.Add(fn.Name, $"@{fn.Name}");
            TempReg = 0;
            Resolver = new MemResolver();
            Resolver.EnterScope();

            CurrentFunction = fn;
            CurrentLabel = new Label("entry");
        }

        void ExitFunction()
        {
            Resolver.ExitScope();
            CurrentFunction.Labels.Add(CurrentLabel);
            Declarations.Add(CurrentFunction);
        }

        void BuildFunction(FunctionDeclaration f)
        {
            var fn = new Function(
                f.FunctionName,
                Int64.Instance,
                f.ArgumentList.Select((_) => Int64.Instance).ToList<DataType>());

            EnterFunction(fn);

            for (var i = 0; i < f.ArgumentList.Count; i++)
            {
                var argument = f.ArgumentList[i];
                var pointer = DeclareVariable(argument, f.SourceStart);

                CurrentLabel.Add(
                    new Alloc(Int64.Instance, pointer)
                );

                CurrentLabel.Add(new Store(
                    Int64.Instance,
                    $"%{i}",
                    Pointer.Int64Ptr,
                    pointer
                ));
            }

            BuildBlock(f.Body);

            if (CurrentLabel.Ops.Count == 0 || CurrentLabel.Ops[^1] is not Ret)
            {
                CurrentLabel.Add(new Ret(Int64.Instance, "0"));
            }

            ExitFunction();
        }
    }
}