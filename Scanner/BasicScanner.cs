using Simp.Common;

namespace Simp.Scanner
{
    public class KeywordLookup
    {
        Dictionary<string, TokenType> Lookup { get; set; } = new Dictionary<string, TokenType>();

        public KeywordLookup()
        {
            Lookup.Add("let", TokenType.Let);
            Lookup.Add("function", TokenType.Function);
            Lookup.Add("if", TokenType.If);
            Lookup.Add("else", TokenType.Else);
            Lookup.Add("while", TokenType.While);
            Lookup.Add("for", TokenType.For);
            Lookup.Add("return", TokenType.Return);
            Lookup.Add("print", TokenType.Print);
            Lookup.Add("exit", TokenType.Exit);
        }

        public bool Contains(string key) => Lookup.ContainsKey(key);
        public TokenType Type(string key) => Lookup[key];
    }

    public class BasicScanner
    {
        static KeywordLookup Keywords { get; set; } = new KeywordLookup();

        string Source { get; set; }
        string SourceFile { get; set; }

        int Start { get; set; }
        int CurrentIndex { get; set; }
        long Line { get; set; } = 1;
        long Column { get; set; } = 1;
        long LineBreak { get; set; }

        public bool IsValid { get; set; } = true;

        bool IsAtEnd() => CurrentIndex >= Source.Length;
        char Current() => IsAtEnd() ? '\0' : Source[CurrentIndex];

        char Advance()
        {
            char val = Current();
            if (val == '\n')
            {
                Line++;
                LineBreak = CurrentIndex;
            }
            CurrentIndex++;
            return val;
        }

        bool Match(char expected)
        {
            if (IsAtEnd() || Current() != expected)
            {
                return false;
            }

            Advance();
            return true;
        }

        Token CreateToken(TokenType type, string literal = "")
        {
            var text = Source[Start..CurrentIndex];
            return new Token(type, text, Line, Column, SourceFile, literal);
        }

        void LogError(TokenError e)
        {
            IsValid = false;
            Console.WriteLine(e.ToString());
        }

        public List<Token> ScanFile(string source, string sourceFileName)
        {
            SourceFile = sourceFileName;
            Source = source;

            var tokens = new List<Token>();

            while (!IsAtEnd())
            {
                // Reset the column position
                if (Start < LineBreak)
                {
                    Column = LineBreak;
                }

                Start = CurrentIndex;

                try
                {
                    var token = Scan();
                    if (token.Type != TokenType.Nop)
                    {
                        tokens.Add(token);
                    }

                }
                catch (TokenError e)
                {
                    LogError(e);
                }
            }

            return tokens;
        }

        Token Scan()
        {
            char c = Advance();
            switch (c)
            {
                case '(': return CreateToken(TokenType.LeftParen);
                case ')': return CreateToken(TokenType.RightParen);
                case '{': return CreateToken(TokenType.LeftBrace);
                case '}': return CreateToken(TokenType.RightBrace);
                case '[': return CreateToken(TokenType.LeftBracket);
                case ']': return CreateToken(TokenType.RightBracket);
                case '+': return CreateToken(TokenType.Plus);
                case '-': return CreateToken(TokenType.Minus);
                case '*': return CreateToken(TokenType.Star);
                case '@': return CreateToken(TokenType.At);
                case ',': return CreateToken(TokenType.Comma);
                case ';': return CreateToken(TokenType.Semicolon);
                case '!':
                    if (Match('='))
                    {
                        return CreateToken(TokenType.BangEqual);
                    }
                    else
                    {
                        return CreateToken(TokenType.Bang);
                    }
                case '=':
                    if (Match('='))
                    {
                        return CreateToken(TokenType.EqualEqual);
                    }
                    else
                    {
                        return CreateToken(TokenType.Equal);
                    }
                case '<':
                    if (Match('='))
                    {
                        return CreateToken(TokenType.LessEqual);
                    }
                    else
                    {
                        return CreateToken(TokenType.Less);
                    }
                case '>':
                    if (Match('='))
                    {
                        return CreateToken(TokenType.GreaterEqual);
                    }
                    else
                    {
                        return CreateToken(TokenType.Greater);
                    }
                case '&':
                    if (Match('&'))
                    {
                        return CreateToken(TokenType.AmpAmp);
                    }
                    else
                    {
                        return CreateToken(TokenType.Amp);
                    }
                case '|':
                    if (Match('|'))
                    {
                        return CreateToken(TokenType.BarBar);
                    }
                    else
                    {
                        return CreateToken(TokenType.Bar);
                    }
                case '/':
                    if (Match('/'))
                    {
                        return ConsumeComment();
                    }
                    else
                    {
                        return CreateToken(TokenType.Slash);
                    }

                case ' ':
                case '\r':
                case '\t':
                case '\n':
                    return Token.Nop;

                default:
                    if (char.IsLetter(c) || c == '_')
                    {
                        return ScanIdentifier();
                    }
                    else if (char.IsNumber(c))
                    {
                        return ScanNumber();
                    }
                    throw new TokenError(
                        $"Unexpected character {c}",
                        CreateToken(TokenType.Nop));
            }
        }

        Token ConsumeComment()
        {
            while (Current() != '\n' && !IsAtEnd())
            {
                Advance();
            }

            return Token.Nop;
        }

        Token ScanIdentifier()
        {
            while (char.IsLetter(Current()) || char.IsNumber(Current()) || Current() == '_')
            {
                Advance();
            }

            var identifier = Source[Start..CurrentIndex];

            if (Keywords.Contains(identifier))
            {
                return CreateToken(Keywords.Type(identifier), identifier);
            }
            else
            {
                return CreateToken(TokenType.Identifier, identifier);
            }
        }

        Token ScanNumber()
        {
            while (char.IsDigit(Current()) && !IsAtEnd())
            {
                Advance();
            }

            var identifier = Source[Start..CurrentIndex];
            return CreateToken(TokenType.IntLiteral, identifier);
        }

    }
}