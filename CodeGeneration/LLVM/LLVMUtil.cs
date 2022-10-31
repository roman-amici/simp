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
            CurrentLabel.Add(new Extension(
                register,
                boolRegister,
                Int64.Instance,
                I1.Instance,
                Extension.ExtensionType.trunc
            ));

            return boolRegister;
        }
    }
}