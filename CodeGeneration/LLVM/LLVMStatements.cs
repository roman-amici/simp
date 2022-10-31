using Simp.AST;
using Simp.Common;

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
                case WhileStatement w:
                    BuildWhile(w);
                    break;
                case LetStatement l:
                    BuildLet(l);
                    break;
                case ExpressionStatement e:
                    BuildExpression(e.Expr);
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

        void BuildWhile(WhileStatement w)
        {
            var comparison = NextLabel();
            var endWhile = NextLabel();
            var whileBody = NextLabel();

            BranchAndEnter(comparison);
            var reg = BuildExpression(w.Predicate);
            var boolReg = CastToI1(reg);

            CurrentLabel.Add(new ConditionalBranch(
                boolReg,
                I1.Instance,
                whileBody.Tag,
                endWhile.Tag
            ));

            EnterLabel(whileBody);
            BuildBlock(w.Block);

            CurrentLabel.Add(new Branch(comparison.Tag));

            EnterLabel(endWhile);
        }

        void BuildLet(LetStatement l)
        {
            var variable = Resolver.DeclareVariable(l.Target.QualifiedName);
            if (variable == null)
            {
                throw new TokenError($"Variable '{l.Target.QualifiedName}' already declared.", l.SourceStart);
            }

            CurrentLabel.Add(new Alloc(Int64.Instance, variable));

            var expr = BuildExpression(l.Initializer);

            CurrentLabel.Add(new Store(
                Int64.Instance,
                expr,
                Pointer.Int64Ptr,
                variable
            ));
        }
    }
}