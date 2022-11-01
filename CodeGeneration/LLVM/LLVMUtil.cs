using Simp.AST;
using Simp.Common;

namespace Simp.CodeGeneration.LLVM
{
    public partial class Builder
    {
        string NextTemp()
        {
            return $"%t{TempReg++}";
        }

        Label NextLabel()
        {
            return new Label($"l{LabelIndex++}");
        }

        void EnterLabel(Label newLabel)
        {
            CurrentFunction.Labels.Add(CurrentLabel);
            CurrentLabel = newLabel;
        }

        void BranchAndEnter(Label newLabel)
        {
            CurrentLabel.Add(new Branch(newLabel.Tag));
            EnterLabel(newLabel);
        }

        string CastToI1(string register)
        {
            var boolRegister = NextTemp();
            CurrentLabel.Add(new Comp(
                "0",
                register,
                boolRegister,
                Int64.Instance,
                Comp.Condition.ne
            ));

            return boolRegister;
        }

        string I1ToInt64(string register)
        {
            var intRegister = NextTemp();
            CurrentLabel.Add(new Extension(
                register,
                intRegister,
                I1.Instance,
                Int64.Instance,
                Extension.ExtensionType.zext
            ));

            return intRegister;
        }

        string DeclareVariable(string name, Token t)
        {
            var variable = Resolver.DeclareVariable(name);
            if (variable == null)
            {
                throw new TokenError($"Variable '{name}' already declared.", t);
            }

            return variable;
        }

        string LookupVariable(string name, Token t)
        {
            var variable = Resolver.Lookup(name);
            if (variable == null)
            {
                if (GlobalVariables.ContainsKey(name))
                {
                    variable = GlobalVariables[name];
                }
                else
                {
                    throw new TokenError(
                        $"Reference to undefined variable {name}", t);
                }
            }

            return variable;
        }

        string LookupVariable(Variable v)
        {
            return LookupVariable(v.Name.QualifiedName, v.SourceStart);
        }
    }
}