using Simp.Common;

namespace Simp.AST
{
    public abstract class ExpressionNode : ASTNode
    {
        public ExpressionNode(Token sourceStart) : base(sourceStart) { }
    }

    public class IntLiteral : ExpressionNode
    {
        public int Value { get; private set; }
        public IntLiteral(Token sourceStart, int value) : base(sourceStart)
        {
            Value = value;
        }
    }

    public class Unary : ExpressionNode
    {
        public ExpressionNode Expr { get; private set; }
        public Token Operator { get; private set; }

        public Unary(
            Token sourceStart,
            ExpressionNode expr,
            Token op
        ) : base(sourceStart)
        {
            Expr = expr;
            Operator = op;
        }
    }

    public class Binary : ExpressionNode
    {
        public ExpressionNode Left { get; private set; }
        public ExpressionNode Right { get; private set; }
        public Token Operator { get; private set; }
        public Binary(
            Token sourceStart,
            ExpressionNode left,
            ExpressionNode right,
            Token op
        ) : base(sourceStart)
        {
            Left = left;
            Right = right;
            Operator = op;
        }
    }

    public class Variable : ExpressionNode
    {
        public Name Name { get; private set; }
        public Variable(Token sourceStart, Name name) : base(sourceStart)
        {
            Name = name;
        }
    }

    public class Call : ExpressionNode
    {
        public Token CallStart { get; private set; }
        public ExpressionNode Callee { get; private set; }
        public IList<ExpressionNode> ArgumentList { get; private set; }

        public Call(
            Token sourceStart,
            Token callStart,
            ExpressionNode callee,
            IList<ExpressionNode> argumentList)
            : base(sourceStart)
        {
            CallStart = callStart;
            Callee = callee;
            ArgumentList = argumentList;
        }
    }

    public class ArrayIndex : ExpressionNode
    {
        public ExpressionNode CallSite { get; private set; }
        public ExpressionNode Index { get; private set; }

        public ArrayIndex(
            Token sourceStart,
            ExpressionNode callSite,
            ExpressionNode index)
            : base(sourceStart)
        {
            CallSite = callSite;
            Index = index;
        }
    }

    public abstract class SetExpression : ExpressionNode
    {
        public ExpressionNode Target { get; private set; }
        public ExpressionNode Value { get; private set; }

        public SetExpression(
            Token sourceStart,
            ExpressionNode target,
            ExpressionNode value)
            : base(sourceStart)
        {
            Target = target;
            Value = value;
        }
    }

    public class Assign : SetExpression
    {
        public Assign(
            Token sourceStart,
            Variable target,
            ExpressionNode value
        ) : base(sourceStart, target, value) { }
    }

    public class ArrayAssign : SetExpression
    {
        public ArrayAssign(
            Token sourceStart,
            ArrayIndex target,
            ExpressionNode value
        ) : base(sourceStart, target, value) { }
    }

}