using Simp.AST;

namespace Simp.CodeGeneration.LLVM
{
    public partial class Builder
    {

        void BuildBlock(BlockStatement block)
        {
            Resolver.EnterScope();
            foreach (var statement in block.Statements)
            {
                BuildStatement(statement);
            }
            Resolver.ExitScope();
        }

        void BuildStatement(Statement s)
        {
            switch (s)
            {
                case ReturnStatement r:
                    BuildReturn(r);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        void BuildReturn(ReturnStatement r)
        {
            if (r.Expr is not null)
            {
                var reg = BuildExpression(r.Expr);
                CurrentLabel.Add(new Ret(Int64.Instance, reg));
            }
            else
            {
                CurrentLabel.Add(new Ret(Int64.Instance, "0"));
            }
        }
    }
}