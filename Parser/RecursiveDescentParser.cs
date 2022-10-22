using Simp.AST;
using Simp.Common;

namespace Simp.Parser
{
    public partial class RecursiveDescentParser
    {
        IList<Token> Tokens { get; set; }
        int Current { get; set; } = 0;
        public bool IsValid { get; private set; } = true;

        bool IsAtEnd() => Current >= Tokens.Count;
        Token Peek() => IsAtEnd() ? Token.Nop : Tokens[Current];
        Token Previous() => Tokens[Current - 1];

        void ParserError(Token token, string errorMessage)
        {
            IsValid = false;
            throw new TokenError(errorMessage, token);
        }

        void ParserWarning(Token token, string errorMessage)
        {
            Console.WriteLine($"{token.PositionLabel()} - Warning: {errorMessage}");
        }

        bool Check(TokenType type)
        {
            return IsAtEnd() ? false : Peek().Type == type;
        }

        Token Advance()
        {
            if (!IsAtEnd())
            {
                Current++;
            }

            return Previous();
        }

        bool Match(params TokenType[] toMatch)
        {
            // Returns true if any of the given tokens are matched.
            foreach (var token in toMatch)
            {
                if (Check(token))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        Token Consume(TokenType type, string errorMessage)
        {
            if (!Check(type))
            {
                ParserError(Peek(), errorMessage);
            }

            return Advance();
        }

        public RecursiveDescentParser(IList<Token> tokens)
        {
            Tokens = tokens;
        }

        public IList<Declaration> Parse()
        {
            List<Declaration> declarations = new List<Declaration>();
            while (!IsAtEnd())
            {
                try
                {
                    declarations.Add(ParseDeclaration());
                }
                catch (TokenError e)
                {
                    Console.WriteLine(e.ToString());
                    // TODO: Synchronization
                    return declarations;
                }
            }

            return declarations;
        }
    }
}