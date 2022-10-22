using Simp.Common;

namespace Simp.AST
{
    public abstract class Declaration : ASTNode
    {
        public Declaration(Token sourceStart) : base(sourceStart) { }
    }

    public class FunctionDeclaration : Declaration
    {
        public string FunctionName { get; private set; }
        public BlockStatement Body { get; private set; }
        public IList<string> ArgumentList { get; private set; }

        public FunctionDeclaration(
            Token sourceStart,
            string functionName,
            IList<string> argumentList,
            BlockStatement body
        ) : base(sourceStart)
        {
            FunctionName = functionName;
            ArgumentList = argumentList;
            Body = body;
        }
    }
}