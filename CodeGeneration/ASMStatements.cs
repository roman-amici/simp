using Simp.AST;

namespace Simp.CodeGeneration
{
    public partial class ASMGenerator
    {
        public void GenStatements(IList<Statement> statements)
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
                    Text.Add("mov rax, 60"); // Exit syscall code
                    Text.Add("pop rdi");
                    Text.Add("syscall");
                    break;
            }
        }
    }
}