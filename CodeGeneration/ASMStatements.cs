using Simp.AST;
using Simp.Common;

namespace Simp.CodeGeneration
{
    public partial class ASMGenerator
    {
        void GenStatements(IList<Statement> statements)
        {
            foreach (var statement in statements)
            {
                GenStatement(statement);
            }
        }

        void GenStatement(Statement statement)
        {
            switch (statement)
            {
                case ExpressionStatement e:
                    GenExpression(e.Expr);
                    Add("pop rax"); // Keep the stack normalized
                    break;
                case ExitStatement ex:
                    GenExitStatement(ex);
                    break;
                case BlockStatement b:
                    GenBlockStatement(b);
                    break;
                case IfStatement i:
                    GenIfStatement(i, null);
                    break;
                case LetStatement l:
                    GenLetStatement(l);
                    break;
                case WhileStatement w:
                    GenWhileStatement(w);
                    break;
                case ForStatement f:
                    GenForStatement(f);
                    break;
                case ArrayDeclaration a:
                    GenArrayDeclaration(a);
                    break;
                case ReturnStatement r:
                    GenReturnStatement(r);
                    break;
            }
        }

        void GenExitStatement(ExitStatement ex)
        {
            GenExpression(ex.Expr);
            Add("mov rax, 60"); // Exit syscall code
            Add("pop rdi");
            Add("syscall");
        }

        void GenBlockStatement(BlockStatement b)
        {
            Resolver.EnterScope();
            GenStatements(b.Statements);
            Resolver.ExitScope();
        }

        void GenIfStatement(IfStatement i, string? endIf)
        {
            var firstIf = endIf == null;
            endIf ??= NextLabel();

            // Not the "final" else
            var nextJump = i.ElseStatement != null ? NextLabel() : endIf;
            if (i.Predicate != null)
            {
                GenExpression(i.Predicate);
                Add("pop rax");
                Add("cmp rax, 0");
                Add($"je {nextJump}");
            }

            GenBlockStatement(i.ThenBlock);

            if (i.ElseStatement != null)
            {
                Add($"je {endIf}"); // Jump at the end of the 
                Add($"{nextJump}:");
                GenIfStatement(i.ElseStatement, endIf);
            }

            if (firstIf)
            {
                Add($"{endIf}:");
            }
        }

        void GenLetStatement(LetStatement let)
        {
            var slot = Resolver.DeclareVariable(let.Target.QualifiedName);
            if (slot == null)
            {
                throw new TokenError($"Variable '{let.Target.QualifiedName}' has already been declared in this scope.", let.SourceStart);
            }

            GenExpression(let.Initializer);
            Add($"pop rax");

            var offset = slot * 8;
            Add($"mov [rbp-{offset}], rax");
        }

        void GenWhileStatement(WhileStatement w)
        {
            var predicateStart = NextLabel();
            var whileEnd = NextLabel();

            AddLabel(predicateStart);
            GenExpression(w.Predicate);
            Add("pop rax");
            Add("cmp rax, 0");
            Add($"je {whileEnd}");
            GenBlockStatement(w.Block);
            Add($"jmp {predicateStart}");
            AddLabel(whileEnd);
        }

        void GenForStatement(ForStatement f)
        {
            var predicateStart = NextLabel();
            var forEnd = NextLabel();

            Resolver.EnterScope();
            GenStatement(f.Initializer);
            AddLabel(predicateStart);
            GenExpression(f.Predicate);
            Add("pop rax");
            Add("cmp rax, 0");
            Add($"je {forEnd}");
            GenBlockStatement(f.Block);
            GenExpression(f.Update);
            Add($"jmp {predicateStart}");
            Resolver.ExitScope();
            AddLabel(forEnd);
        }

        void GenArrayDeclaration(ArrayDeclaration a)
        {
            var (variableSlot, arrayStart) = Resolver.DeclareArray(a.Target.QualifiedName, a.Size);

            var variableOffset = variableSlot * 8;
            var arrayStartOffset = arrayStart * 8;
            Add($"mov rax, rbp");
            Add($"add rax, -{arrayStartOffset}");
            Add($"mov [rbp-{variableOffset}], rax");
        }

        void GenReturnStatement(ReturnStatement r)
        {
            if (r.Expr is not null)
            {
                GenExpression(r.Expr);
                Add("pop rax");
            }

            Add($"jmp {FunctionReturnLabel}");
        }
    }
}