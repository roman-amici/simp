using Simp.Common;

namespace Simp.AST
{
    public abstract class Declaration : ASTNode
    {
        public Declaration(Token sourceStart) : base(sourceStart) { }
    }

    public class FunctionDeclaration : Declaration
    {
        public Token FunctionName { get; private set; }
        public BlockStatement Body { get; private set; }

        public FunctionDeclaration(
            Token sourceStart,
            Token functionName,
            BlockStatement body
        ) : base(sourceStart)
        {
            FunctionName = functionName;
            Body = body;
        }
    }
}