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
                case Variable v:
                    GenVariable(v);
                    break;
                case Assign a:
                    GenAssign(a);
                    break;
            }
        }

        void GenUnary(Unary u)
        {
            GenExpression(u.Expr);
            switch (u.Operator.Type)
            {
                case TokenType.Minus:
                    Add("pop rax");
                    Add("neg rax");
                    Add("push rax");
                    break;
                case TokenType.Bang:
                    Add("pop rax");
                    Add("xor rbx, rbx");
                    Add("cmp rax, 0");
                    Add("setz bl");
                    Add("push rbx");
                    break;
                case TokenType.Amp:
                    GenAddress(u.Expr);
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

            Add("pop rbx");
            Add("pop rax");

            switch (b.Operator.Type)
            {
                case TokenType.Plus:
                    Add("add rax, rbx");
                    Add("push rax");
                    break;
                case TokenType.Minus:
                    Add("sub rax, rbx");
                    Add("push rax");
                    break;
                case TokenType.Star:
                    Add("imul rax, rbx");
                    Add("push rax");
                    break;
                case TokenType.Slash:
                    Clear("rdx");
                    Add("idiv rbx"); // divide by rbx
                    Add("push rax");
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

            Add("mov rax, [rsp]");
            Add("cmp rax, 0");

            if (b.Operator.Type == TokenType.AmpAmp)
            {
                Add($"je {skipLabel}");
            }
            else if (b.Operator.Type == TokenType.BarBar)
            {
                Add($"jne {skipLabel}");
            }

            // Fall through to expression 2
            // Remove the old value from the stack since its irrelevant if we fall through;
            Add("pop rax");

            GenExpression(b.Right);
            Add($"{skipLabel}:");

        }

        void GenInt(IntLiteral l)
        {
            Add($"push {l.Value}");
        }

        void GenVariable(Variable v)
        {
            var slot = Resolver.FindSlot(v.Name.QualifiedName);
            if (slot == null)
            {
                throw new TokenError($"Reference to undeclared variable '{v.Name.QualifiedName}'", v.SourceStart);
            }

            var offset = slot * 8;
            Add($"mov rax, [rbp-{offset}]");
            Add($"push rax");
        }

        void GenAssign(Assign a)
        {
            var variable = a.Target as Variable;
            var slot = Resolver.FindSlot(variable.Name.QualifiedName);
            if (slot == null)
            {
                throw new TokenError($"Assignment to undeclared variable '{variable.Name.QualifiedName}'", a.SourceStart);
            }

            var offset = slot * 8;
            GenExpression(a.Value);
            Add("mov rax, [rsp]");
            Add($"mov [rbp-{offset}], rax");
        }

        void GenAddress(ExpressionNode e)
        {
            if (e is Variable v)
            {
                var slot = Resolver.FindSlot(v.Name.QualifiedName);
                if (slot == null)
                {
                    throw new TokenError($"Reference to undeclared variable '{v.Name.QualifiedName}'", v.SourceStart);
                }

                var offset = slot * 8;
                Add($"lea rax, [ebp-{offset}]");
                Add("push rax");
            }
            else
            {
                throw new TokenError("Can't take reference to expression.", e.SourceStart);
            }
        }
    }
}