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
                TokenType.If => IfStatement(),
                TokenType.LeftBrace => BlockStatement(),
                TokenType.Let => LetStatement(),
                TokenType.While => WhileStatement(),
                TokenType.For => ForStatement(),
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

        public BlockStatement BlockStatement()
        {
            var brace = Consume(TokenType.LeftBrace, "Expected '{'.");

            var statements = new List<Statement>();
            while (!Match(TokenType.RightBrace))
            {
                statements.Add(ParseStatement());
            }

            return new BlockStatement(brace, statements);
        }

        public IfStatement IfStatement()
        {
            var ifToken = Consume(TokenType.If, "Expected 'if'.");

            Consume(TokenType.LeftParen, "Expected '(' after 'if'.");
            var predicate = Expr();
            Consume(TokenType.RightParen, "Expected ')' after predicate.");

            var block = BlockStatement();

            IfStatement? elseStatement = null;
            if (Match(TokenType.Else))
            {
                var elseToken = Previous();
                if (Peek().Type == TokenType.If)
                {
                    elseStatement = IfStatement();
                }
                else
                {
                    var elseBlock = BlockStatement();
                    elseStatement = new IfStatement(elseToken, null, elseBlock, null);
                }
            }

            return new IfStatement(ifToken, predicate, block, elseStatement);
        }

        public Statement LetStatement()
        {
            var letToken = Consume(TokenType.Let, "Expected 'let'.");
            var name = Consume(TokenType.Identifier, "Expected identifier after 'let'.");

            if (Match(TokenType.LeftBracket))
            {
                var sizeToken = Consume(TokenType.IntLiteral, "Expected array size after '['.");
                var size = int.Parse(sizeToken.Literal);

                Consume(TokenType.RightBracket, "Expected ']'.");
                Consume(TokenType.Semicolon, "Expected ';'");
                return new ArrayDeclaration(letToken, new Name(name.Literal), size);
            }
            else
            {
                Consume(TokenType.Equal, "Expected '=' after identifier.");
                var initializer = Expr();

                Consume(TokenType.Semicolon, "Expected ';'.");

                return new LetStatement(letToken, new Name(name.Literal), initializer);
            }
        }

        public WhileStatement WhileStatement()
        {
            var whileToken = Consume(TokenType.While, "Expected 'while'.");
            Consume(TokenType.LeftParen, "Expected '(' after 'while'.");

            var predicate = Expr();

            Consume(TokenType.RightParen, "Expected ')'.");

            var block = BlockStatement();

            return new WhileStatement(
                whileToken,
                predicate,
                block);
        }

        public ForStatement ForStatement()
        {
            var forToken = Consume(TokenType.For, "Expected 'for'.");

            Consume(TokenType.LeftParen, "Expected '(' after 'for'.");

            var initializer = ParseStatement();
            var predicate = Expr();
            Consume(TokenType.Semicolon, "Expected ';'");
            var update = Expr();

            Consume(TokenType.RightParen, "Expected ')'");

            var block = BlockStatement();

            return new ForStatement(
                forToken,
                initializer,
                predicate,
                update,
                block
            );
        }
    }

}