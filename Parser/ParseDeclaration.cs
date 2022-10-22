using Simp.AST;
using Simp.Common;

namespace Simp.Parser
{
    public partial class RecursiveDescentParser
    {

        Declaration ParseDeclaration()
        {
            return Peek().Type switch
            {
                TokenType.Function => FunctionDeclaration(),
                _ => throw new TokenError("Unexpected Token", Peek())
            };
        }

        FunctionDeclaration FunctionDeclaration()
        {
            var functionToken = Consume(TokenType.Function, "Expected 'function'.");
            var functionName = Consume(TokenType.Identifier, "Expected function name.");

            Consume(TokenType.LeftParen, "Expected '('");
            var argumentList = new List<string>();
            while (!Check(TokenType.RightParen))
            {
                do
                {
                    var arg = Consume(TokenType.Identifier, "Expected identifier.");
                    argumentList.Add(arg.Lexeme);
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, "Expected ')' after arguments list.");

            var functionBody = BlockStatement();

            return new FunctionDeclaration(
                functionToken,
                functionName.Lexeme,
                argumentList,
                functionBody
            );
        }
    }
}