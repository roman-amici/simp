using Simp.Common;

namespace Simp.AST
{
    public abstract class Statement : ASTNode
    {
        public Statement(Token sourceStart) : base(sourceStart) { }
    }

    public class LetStatement : Statement
    {
        public Name Target { get; private set; }
        public ExpressionNode Initializer { get; private set; }

        public LetStatement(
            Token sourceStart,
            Name target,
            ExpressionNode initializer)
            : base(sourceStart)
        {
            Target = target;
            Initializer = initializer;
        }
    }

    public class ArrayDeclaration : Statement
    {
        public Name Target { get; private set; }
        public int Size { get; private set; }

        public ArrayDeclaration(
            Token sourceStart,
            Name target,
            int size)
            : base(sourceStart)
        {
            Target = target;
            Size = size;
        }
    }

    public class ExpressionStatement : Statement
    {
        public ExpressionNode Expr { get; private set; }
        public ExpressionStatement(
            Token sourceStart,
            ExpressionNode expr
        ) : base(sourceStart)
        {
            Expr = expr;
        }
    }

    public class PrintStatement : Statement
    {
        public ExpressionNode Expr { get; private set; }
        public PrintStatement(
            Token sourceStart,
            ExpressionNode expr
        ) : base(sourceStart)
        {
            Expr = expr;
        }
    }

    public class ExitStatement : Statement
    {
        public ExpressionNode Expr { get; private set; }
        public ExitStatement(
            Token sourceStart,
            ExpressionNode expr
        ) : base(sourceStart)
        {
            Expr = expr;
        }
    }

    public class BlockStatement : Statement
    {
        public IList<Statement> Statements { get; private set; }
        public BlockStatement(
            Token sourceStart,
            IList<Statement> statements
        ) : base(sourceStart)
        {
            Statements = statements;
        }
    }

    public class WhileStatement : Statement
    {
        public BlockStatement Block { get; private set; }
        public ExpressionNode Predicate { get; private set; }

        public WhileStatement(
            Token sourceStart,
            ExpressionNode predicate,
            BlockStatement block
        ) : base(sourceStart)
        {
            Block = block;
            Predicate = predicate;
        }
    }

    public class ForStatement : Statement
    {
        public Statement Initializer { get; private set; }
        public ExpressionNode Predicate { get; private set; }
        public ExpressionNode Update { get; private set; }
        public BlockStatement Block { get; private set; }

        public ForStatement(
            Token sourceStart,
            Statement initializer,
            ExpressionNode predicate,
            ExpressionNode update,
            BlockStatement block
        ) : base(sourceStart)
        {
            Initializer = initializer;
            Predicate = predicate;
            Update = update;
            Block = block;
        }
    }

    public class IfStatement : Statement
    {
        public ExpressionNode? Predicate { get; private set; }
        public BlockStatement ThenBlock { get; private set; }
        public IfStatement? ElseStatement { get; private set; }

        public IfStatement(
            Token sourceStart,
            ExpressionNode? predicate,
            BlockStatement thenBlock,
            IfStatement? elseStatement
        ) : base(sourceStart)
        {
            Predicate = predicate;
            ThenBlock = thenBlock;
            ElseStatement = elseStatement;
        }
    }

    public class ReturnStatement : Statement
    {
        public ExpressionNode Expr { get; private set; }
        public ReturnStatement(
            Token sourceStart,
            ExpressionNode expr
        ) : base(sourceStart)
        {
            Expr = expr;
        }
    }
}