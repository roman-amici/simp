using Simp.AST;

namespace Simp.CodeGeneration.LLVM
{
    public partial class Builder
    {
        string BuildExpression(ExpressionNode expr)
        {
            return expr switch
            {
                IntLiteral i => BuildIntLiteral(i),
                _ => throw new NotImplementedException()
            };
        }

        string BuildIntLiteral(IntLiteral i)
        {
            return i.Value.ToString();
        }
    }
}