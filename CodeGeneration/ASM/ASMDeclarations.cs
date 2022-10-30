using Simp.AST;
using Simp.Common;
using Simp.StaticAnalysis;

namespace Simp.CodeGeneration.ASM
{
    public partial class ASMGenerator
    {
        void GenDeclaration(Declaration declaration)
        {
            switch (declaration)
            {
                case FunctionDeclaration f:
                    GenFunction(f);
                    break;
                default:
                    throw new NotImplementedException("Unknown declaration.");
            }
        }

        void EnterFunction()
        {
            FunctionText = new List<string>();
            Resolver = new Resolver();
            FunctionReturnLabel = NextLabel();
        }

        void ExitFunction()
        {
            var offset = Resolver.MaxSlots * 8;

            // Function preamble
            Text.Add("push rbp");
            Text.Add("mov rbp, rsp");
            Text.Add($"sub rsp, ${offset}");

            foreach (var line in FunctionText)
            {
                Text.Add(line);
            }

            Text.Add($"{FunctionReturnLabel}:");
            Text.Add("leave");
            Text.Add("ret");
        }

        public void GenFunction(FunctionDeclaration function)
        {
            if (GlobalResolver.ContainsKey(function.FunctionName))
            {
                throw new TokenError("Duplicate function declaration", function.SourceStart);
            }

            GlobalResolver[function.FunctionName] = function;
            Text.Add($"{function.FunctionName}:");

            EnterFunction();

            foreach (var argument in function.ArgumentList)
            {
                Resolver.DeclareFunctionArgument(argument);
            }
            GenBlockStatement(function.Body);
            ExitFunction();
        }
    }
}
