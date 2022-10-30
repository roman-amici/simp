using Simp.AST;
using Simp.Common;
using Simp.StaticAnalysis;

namespace Simp.CodeGeneration.LLVM
{
    public partial class Builder
    {
        List<LLVMDeclaration> Declarations { get; set; } = new();

        Function CurrentFunction { get; set; }
        Label CurrentLabel { get; set; }

        MemResolver Resolver { get; set; }

        public bool Build(IList<Declaration> declarations)
        {
            try
            {
                foreach (var declaration in declarations)
                {
                    BuildDeclaration(declaration);
                }
            }
            catch (TokenError e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public void Generate(string fileName)
        {
            using var stream = new StreamWriter(fileName);
            foreach (var declaration in Declarations)
            {
                declaration.Generate(stream);
            }
        }
    }
}