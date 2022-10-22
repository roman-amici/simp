namespace Simp.CodeGeneration
{
    public partial class ASMGenerator
    {
        void Clear(string reg)
        {
            Add($"xor {reg},{reg}");
        }

        void SetCompare(string set)
        {
            Clear("rdx");
            Add("cmp rax, rbx");
            Add($"{set} dl");
            Add("push rdx");
        }

        void Add(string value)
        {
            FunctionText.Add(value);
        }

        void AddLabel(string value)
        {
            Add($"{value}:");
        }
    }
}