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
                case ArrayIndex x:
                    GenArrayIndex(x);
                    break;
                case ArrayAssign g:
                    GenArrayAssign(g);
                    break;
                case Call c:
                    GenFunctionCall(c);
                    break;
            }
        }

        void GenUnary(Unary u)
        {
            GenExpression(u.Expr);
            switch (u.Operator.Type)
            {
                case TokenType.Minus:
                    Pop("rax");
                    Add("neg rax");
                    Add("push rax");
                    break;
                case TokenType.Bang:
                    Pop("rax");
                    Add("xor rbx, rbx");
                    Add("cmp rax, 0");
                    Add("setz bl");
                    Add("push rbx");
                    break;
                case TokenType.Amp:
                    GenAddress(u.Expr);
                    break;
                case TokenType.At:
                    Pop("rax");
                    Add("mov rax, [rax]");
                    Add("push rax");
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

            Pop("rbx");
            Pop("rax");

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
            Pop("rax");

            GenExpression(b.Right);
            Add($"{skipLabel}:");

        }

        void GenInt(IntLiteral l)
        {
            Add($"push {l.Value}");
        }

        void GenVariable(Variable v)
        {
            var name = v.Name.QualifiedName;

            var slot = Resolver.FindSlot(name);
            if (slot != null)
            {
                var offset = slot * 8;
                Add($"mov rax, [rbp-{offset}]");
                Add($"push rax");
            }
            else if (
                GlobalResolver.ContainsKey(name) &&
                GlobalResolver[name] is FunctionDeclaration)
            {
                Add($"lea rax, [rel {name}]");
                Add("push rax");
            } // TODO: Globals
            else
            {
                throw new TokenError($"Reference to undeclared variable '{v.Name.QualifiedName}'", v.SourceStart);
            }
        }

        void GenAssign(Assign a)
        {
            var variable = (Variable)a.Target;
            var name = variable.Name.QualifiedName;
            var slot = Resolver.FindSlot(name);
            if (slot != null)
            {
                var offset = slot * 8;
                GenExpression(a.Value);
                Add("mov rax, [rsp]");
                Add($"mov [rbp-{offset}], rax");
            }
            else if (
                GlobalResolver.ContainsKey(name) &&
                GlobalResolver[name] is FunctionDeclaration)
            {
                throw new TokenError("Assignment to function is not allowed.", a.SourceStart);
            } // TODO: Globals
            else
            {
                throw new TokenError($"Assignment to undeclared variable '{name}'", a.SourceStart);
            }


        }

        void GenAddress(ExpressionNode e)
        {
            if (e is Variable v)
            {
                var name = v.Name.QualifiedName;
                var slot = Resolver.FindSlot(name);
                if (slot != null)
                {
                    var offset = slot * 8;
                    Add($"lea rax, [rbp-{offset}]");
                    Add("push rax");
                }
                else if (
                    GlobalResolver.ContainsKey(name) &&
                    GlobalResolver[name] is FunctionDeclaration)
                {
                    throw new TokenError($"Cannot take a reference to a function pointer.", v.SourceStart);
                }
                else
                {
                    throw new TokenError($"Reference to undeclared variable '{name}'", v.SourceStart);
                }
            }
            else if (e is ArrayIndex a)
            {
                GenArrayAddress(a);
            }
            else
            {
                throw new TokenError("Can't take reference to expression.", e.SourceStart);
            }
        }

        void GenArrayIndex(ArrayIndex x)
        {
            GenExpression(x.CallSite);
            GenExpression(x.Index);
            Pop("rbx"); // Index
            Pop("rax"); // Pointer
            Add("mov rax, [rax+rbx*8]");
            Add("push rax");
        }

        void GenArrayAddress(ArrayIndex x)
        {
            GenExpression(x.CallSite);
            GenExpression(x.Index);
            Pop("rbx"); // Index
            Pop("rax"); // Pointer
            Add("lea rax, [rax+rbx*8]");
            Add("push rax");
        }

        void GenArrayAssign(ArrayAssign g)
        {
            GenExpression(g.Value);
            GenArrayAddress((ArrayIndex)g.Target);
            Pop("rax"); // Pointer
            Pop("rbx"); // Value
            Add("mov [rax], rbx");
            Add("push rbx");
        }

        void GenFunctionCall(Call c)
        {
            // Add in reverse order
            foreach (var argument in c.ArgumentList.Reverse())
            {
                GenExpression(argument);
            }

            GenExpression(c.Callee);
            Pop("rax");
            Add("call rax");

            var argumentOffset = c.ArgumentList.Count * 8;
            Add($"add rsp, {argumentOffset}");

            Add("push rax"); // Push the return value
        }
    }
}