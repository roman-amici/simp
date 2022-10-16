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
                case Unary u:
                    GenUnary(u);
                    break;
                case Binary b:
                    GenBinary(b);
                    break;
            }
        }

        void GenUnary(Unary u)
        {
            GenExpression(u.Expr);
            switch (u.Operator.Type)
            {
                case TokenType.Minus:
                    Text.Add("pop rax");
                    Text.Add("neg rax");
                    Text.Add("push rax");
                    break;
                case TokenType.Bang:
                    Text.Add("pop rax");
                    Text.Add("xor rbx, rbx");
                    Text.Add("cmp rax, 0");
                    Text.Add("setz bl");
                    Text.Add("push rbx");
                    break;
                default:
                    throw new TokenError("Unexpected operator", u.Operator);
            }
        }

        void GenBinary(Binary b)
        {
            if (b.Operator.Type == TokenType.AmpAmp ||
                b.Operator.Type == TokenType.BarBar)
            {
                GenBinaryShortCircuit(b);
                return;
            }

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
                    Clear("rdx");
                    Text.Add("idiv rbx"); // divide by rbx
                    Text.Add("push rax");
                    break;
                case TokenType.EqualEqual:
                    SetCompare("setz");
                    break;
                case TokenType.BangEqual:
                    SetCompare("setne");
                    break;
                case TokenType.Greater:
                    SetCompare("setg");
                    break;
                case TokenType.GreaterEqual:
                    SetCompare("setge");
                    break;
                case TokenType.Less:
                    SetCompare("setl");
                    break;
                case TokenType.LessEqual:
                    SetCompare("setle");
                    break;
                default:
                    throw new TokenError("Unexpected operator", b.Operator);
            }
        }

        void GenBinaryShortCircuit(Binary b)
        {
            GenExpression(b.Left);
            var skipLabel = NextLabel();

            Text.Add("mov rax, [rsp]");
            Text.Add("cmp rax, 0");

            if (b.Operator.Type == TokenType.AmpAmp)
            {
                Text.Add($"je {skipLabel}");
            }
            else if (b.Operator.Type == TokenType.BarBar)
            {
                Text.Add($"jne {skipLabel}");
            }

            // Fall through to expression 2
            // Remove the old value from the stack since its irrelevant if we fall through;
            Text.Add("pop rax");

            GenExpression(b.Right);
            Text.Add($"{skipLabel}:");

        }

        void GenInt(IntLiteral l)
        {
            Text.Add($"push {l.Value}");
        }
    }
}