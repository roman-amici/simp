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
                case IfStatement i:
                    BuildIf(i, null);
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

        void BuildIf(IfStatement i, Label? branchEnd)
        {
            branchEnd ??= NextLabel();

            if (i.Predicate != null)
            {
                var ifFalseLabel = i.ElseStatement == null ? branchEnd : NextLabel();
                var ifTrueLabel = NextLabel();
                var reg = BuildExpression(i.Predicate);
                var boolReg = NextTemp();

                // Convert from int to bool
                CurrentLabel.Add(new Extension(
                    reg,
                    boolReg,
                    Int64.Instance,
                    I1.Instance,
                    Extension.ExtensionType.trunc));

                CurrentLabel.Add(new ConditionalBranch(
                    boolReg,
                    I1.Instance,
                    ifTrueLabel.Tag,
                    ifFalseLabel.Tag
                ));

                EnterLabel(ifTrueLabel);
                BuildBlock(i.ThenBlock);

                CurrentLabel.Add(new Branch(branchEnd.Tag));

                if (i.ElseStatement != null)
                {
                    EnterLabel(ifFalseLabel);
                    BuildIf(i.ElseStatement, branchEnd);
                }
                else // If with no else
                {
                    EnterLabel(branchEnd);
                }
            }
            else // Final Else
            {
                BuildBlock(i.ThenBlock);
                EnterLabel(branchEnd);
            }
        }
    }
}