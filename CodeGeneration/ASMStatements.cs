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
                    break;
                case ExitStatement ex:
                    GenExpression(ex.Expr);
                    Add("mov rax, 60"); // Exit syscall code
                    Add("pop rdi");
                    Add("syscall");
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
            }
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
    }
}