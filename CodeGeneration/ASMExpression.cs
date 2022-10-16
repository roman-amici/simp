using Simp.AST;
using Simp.Common;

namespace Simp.CodeGeneration
{
    public partial class ASMGenerator
    {
        void GenExpression(ExpressionNode expr)
        {
            switch (expr)
            {
                case IntLiteral l:
                    GenInt(l);
                    break;
                case Binary b:
                    GenBinary(b);
                    break;
            }
        }

        void GenBinary(Binary b)
        {
            GenExpression(b.Left);
            GenExpression(b.Right);

            Text.Add("pop rbx");
            Text.Add("pop rax");

            switch (b.Operator.Type)
            {
                case TokenType.Plus:
                    Text.Add("add rax, rbx");
                    Text.Add("push rax");
                    break;
                case TokenType.Minus:
                    Text.Add("sub rax, rbx");
                    Text.Add("push rax");
                    break;
                case TokenType.Star:
                    Text.Add("imul rax, rbx");
                    Text.Add("push rax");
                    break;
                case TokenType.Slash:
                    Text.Add("xor rdx, rdx");
                    Text.Add("idiv rbx"); // divide by rbx
                    Text.Add("push rax");
                    break;
            }
        }

        void GenInt(IntLiteral l)
        {
            Text.Add($"push {l.Value}");
        }
    }
}