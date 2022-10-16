using Simp.AST;
using Simp.Common;

namespace Simp.Parser
{
    public partial class RecursiveDescentParser
    {
        public Statement ParseStatement()
        {
            return Peek().Type switch
            {
                TokenType.Exit => ExitStatement(),
                _ => ExpressionStatement()
            };
        }

        public ExitStatement ExitStatement()
        {
            var exitToken = Consume(TokenType.Exit, "Expected 'exit'.");
            var expr = Expr();
            Consume(TokenType.Semicolon, "Expected ';'.");

            return new ExitStatement(exitToken, expr);
        }

        public ExpressionStatement ExpressionStatement()
        {
            var expr = Expr();
            Consume(TokenType.Semicolon, "Expected ';'.");

            return new ExpressionStatement(expr.SourceStart, expr);
        }
    }

}