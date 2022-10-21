using Simp.AST;
using Simp.Common;

namespace Simp.Parser
{
    public partial class RecursiveDescentParser
    {
        public ExpressionNode Expr()
        {
            return Assignment();
        }

        public ExpressionNode Assignment()
        {
            var expr = Or();

            if (Match(TokenType.Equal))
            {
                switch (expr)
                {
                    case Variable v:
                        return new Assign(v.SourceStart, v, Expr());
                    case ArrayIndex a:
                        return new ArrayAssign(a.SourceStart, a, Expr());
                    default:
                        ParserError(expr.SourceStart, $"Cannot assign to {expr.SourceStart.Lexeme}");
                        return expr;
                }
            }
            else
            {
                return expr;
            }
        }

        public ExpressionNode Or()
        {
            var left = And();

            while (Match(TokenType.BarBar))
            {
                var op = Previous();
                left = new Binary(left.SourceStart, left, And(), op);
            }

            return left;
        }

        public ExpressionNode And()
        {
            var left = Equality();

            while (Match(TokenType.AmpAmp))
            {
                var op = Previous();
                left = new Binary(left.SourceStart, left, Equality(), op);
            }

            return left;
        }

        public ExpressionNode Equality()
        {
            var left = Comparison();

            while (Match(TokenType.EqualEqual, TokenType.BangEqual))
            {
                var op = Previous();
                left = new Binary(left.SourceStart, left, Comparison(), op);
            }

            return left;
        }

        public ExpressionNode Comparison()
        {
            var left = Bitwise();

            while (Match(
                TokenType.Less,
                TokenType.Greater,
                TokenType.LessEqual,
                TokenType.GreaterEqual))
            {
                var op = Previous();
                left = new Binary(left.SourceStart, left, Bitwise(), op);
            }

            return left;
        }

        public ExpressionNode Bitwise()
        {
            return Term();
        }

        public ExpressionNode Term()
        {
            var left = Factor();
            while (Match(TokenType.Minus, TokenType.Plus))
            {
                var op = Previous();
                left = new Binary(left.SourceStart, left, Factor(), op);
            }

            return left;
        }

        public ExpressionNode Factor()
        {
            var left = Unary();

            while (Match(TokenType.Star, TokenType.Slash))
            {
                var op = Previous();
                left = new Binary(left.SourceStart, left, Unary(), op);
            }

            return left;
        }

        public ExpressionNode Unary()
        {
            if (Match(
                TokenType.Bang,
                TokenType.Minus,
                TokenType.At,
                TokenType.Amp))
            {
                var op = Previous();
                return new Unary(op, Unary(), op);
            }

            return Call();
        }

        public ExpressionNode Call()
        {
            var expr = Primary();

            var done = false;
            while (!done)
            {
                switch (Peek().Type)
                {
                    case TokenType.LeftParen:
                        expr = FunctionCall(expr);
                        break;
                    case TokenType.LeftBracket:
                        expr = ArrayIndex(expr);
                        break;
                    default:
                        done = true;
                        break;
                }
            }

            return expr;
        }

        public Call FunctionCall(ExpressionNode callee)
        {
            var callStart = Consume(TokenType.LeftParen, "Expected '('");

            var argumentList = new List<ExpressionNode>();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    argumentList.Add(Expr());
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, "Expected ')' after arguments");
            return new Call(callStart, callStart, callee, argumentList);
        }

        public ArrayIndex ArrayIndex(ExpressionNode callee)
        {
            var indexStart = Consume(TokenType.LeftBracket, "Expected '['");
            var expr = Expr();

            Consume(TokenType.RightBracket, "Expected ']' after expression");
            return new ArrayIndex(indexStart, callee, expr);
        }

        public ExpressionNode Primary()
        {
            switch (Peek().Type)
            {
                case TokenType.LeftParen:
                    Advance();
                    var inner = Expr();
                    Consume(TokenType.RightParen, "Mismatched parenthesis.");
                    return inner;
                case TokenType.IntLiteral:
                    var number = Advance();
                    return new IntLiteral(number, int.Parse(number.Literal));
                case TokenType.Identifier:
                    var identifier = Advance();
                    return new Variable(
                        identifier,
                        new Name(identifier.Literal));
                default:
                    ParserError(Peek(), "Unexpected token.");
                    throw new InvalidOperationException();
            }
        }
    }
}