using Simp.Common;

namespace Simp.AST
{
    public abstract class ASTNode
    {
        public Token SourceStart { get; private set; }

        public ASTNode(Token sourceStart)
        {
            SourceStart = sourceStart;
        }
    }
}