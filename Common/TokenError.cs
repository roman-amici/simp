namespace Simp.Common
{
    public class TokenError : Exception
    {
        Token Token { get; set; }
        public TokenError(string message, Token token) : base(message)
        {
            Token = token;
        }

        public override string ToString()
        {
            return $"{Token.SourceFile}:{Token.Line}.{Token.Column} - {Message}";
        }
    }
}