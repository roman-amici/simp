namespace Simp.Common
{
    public class Token
    {
        public static Token Nop { get; set; } = new Token(TokenType.Nop, string.Empty, 0, 0, string.Empty, string.Empty);

        public TokenType Type { get; private set; }
        public string Lexeme { get; private set; } = string.Empty;
        public long Line { get; private set; }
        public long Column { get; private set; }
        public string SourceFile { get; private set; } = string.Empty;
        public string Literal { get; private set; }

        public Token(
            TokenType type,
            string lexeme,
            long line,
            long column,
            string sourceFile,
            string literal)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Column = column;
            SourceFile = sourceFile;
            Literal = literal;
        }

        public string PositionLabel()
        {
            return $"{SourceFile}:{Line}:{Column}";
        }
    }
}