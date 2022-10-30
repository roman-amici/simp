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
    }
}