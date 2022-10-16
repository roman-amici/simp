namespace Simp.Common
{
    public enum TokenType
    {
        // Single Char Tokens
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        RightBracket,
        LeftBracket,
        Plus,
        Minus,
        Start,
        Slash,
        Star,
        At,
        Equal,
        Amp,
        Bar,
        Comma,
        Semicolon,

        // Multi char tokens
        Bang,
        BangEqual,
        EqualEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        AmpAmp,
        BarBar,

        // Literals
        Identifier,
        IntLiteral,

        // Keywords
        Let,
        Function,
        If,
        Else,
        While,
        For,
        Return,
        Print,
        Exit,

        Nop

    }
}